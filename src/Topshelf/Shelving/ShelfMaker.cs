// Copyright 2007-2010 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Shelving
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Threading;
    using log4net;
    using Magnum.Channels;
    using Magnum.Extensions;
    using Magnum.Fibers;
    using Magnum.Threading;
    using Messages;

    public class ShelfMaker :
        IDisposable
    {
        readonly WcfUntypedChannelHost _myChannelHost;
        readonly UntypedChannelAdapter _myChannel;
        readonly ReaderWriterLockedObject<Dictionary<string, ShelfHandle>> _shelves;
        static readonly ILog _log = LogManager.GetLogger(typeof(ShelfMaker));

        public ShelfMaker()
        {
            _shelves = new ReaderWriterLockedObject<Dictionary<string, ShelfHandle>>(new Dictionary<string, ShelfHandle>());

            _myChannel = new UntypedChannelAdapter(new ThreadPoolFiber());
            _myChannelHost = new WcfUntypedChannelHost(new SynchronousFiber(), _myChannel, WellknownAddresses.HostAddress, "topshelf.host");

            _myChannel.Subscribe(s =>
            {
                s.Consume<ShelfReady>().Using(m => MarkShelfReadyAndInitService(m));
                s.Consume<ServiceReady>().Using(m => MarkServiceReadyAndStart(m));
                s.Consume<ServiceStopped>().Using(m => MarkShelvedServiceStopped(m));
                s.Consume<ServiceStarted>().Using(m => HandleShelfStateChange(m, ShelfState.Started));
                s.Consume<FileSystemChange>().Using(m => ReloadShelf(m));
                s.Consume<ServiceContinued>().Using(m => HandleShelfStateChange(m, ShelfState.Continued));
                s.Consume<ServicePaused>().Using(m => HandleShelfStateChange(m, ShelfState.Paused));
                s.Consume<ServiceStarting>().Using(m => HandleShelfStateChange(m, ShelfState.Starting));
                s.Consume<ServiceStopping>().Using(m => HandleShelfStateChange(m, ShelfState.Stopping));
                s.Consume<ShelfFault>().Using(m => HandleShelfFault(m));
                s.Consume<ServicePausing>().Using(m => HandleShelfStateChange(m, ShelfState.Pausing));
                s.Consume<ServiceContinuing>().Using(m => HandleShelfStateChange(m, ShelfState.Continuing));
            });
        }

        static ShelfHandle GetShelfStatus(IDictionary<string, ShelfHandle> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        void ReloadShelf(ServiceMessage message)
        {
            try
            {
                ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
                if (shelf != null)
                {
                    if (shelf.CurrentState == ShelfState.Started)
                    {
                        ManualResetEvent resetEvent = shelf.StopHandle = shelf.StopHandle ?? new ManualResetEvent(false);

                        StopShelf(message.ShelfName);

                        resetEvent.WaitOne(30.Seconds());
                    }

                    AppDomain.Unload(shelf.AppDomain);
                    _shelves.WriteLock(dict => dict.Remove(message.ShelfName));
                }

                MakeShelf(message.ShelfName);
            }
            catch (Exception ex)
            {
                // trash the partially formed shelf
                // another file system event might kick it up again
                _shelves.WriteLock(dict => { if (dict.ContainsKey(message.ShelfName)) dict.Remove(message.ShelfName); });
                _log.ErrorFormat("Error reloading shelf {0}: {1}", message.ShelfName, ex);
            }

        }

        public void MakeShelf(string name, params AssemblyName[] assemblies)
        {
            MakeShelf(name, null, assemblies);
        }

        void MarkShelvedServiceStopped(ServiceStopped message)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelf == null)
                return;

            if (shelf.StopHandle != null)
                shelf.StopHandle.Set();

            shelf.CurrentState = ShelfState.Stopped;
        }

        void HandleShelfFault(ShelfFault message)
        {
            _log.Error("Error in shelf {0}: {1}".FormatWith(message.ShelfName, message.Exception));
            ShelfHandle shelfStatus = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelfStatus == null)
                return;

            ReloadShelf(message);
        }

        void HandleShelfStateChange(ServiceMessage message, ShelfState newState)
        {
            ShelfHandle shelfStatus = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelfStatus == null)
                return;

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = newState;

            StateChanged(oldState, newState, message.ShelfName);
        }

        public void MakeShelf(string name, Type bootstrapper, params AssemblyName[] assemblies)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, name));
            if (shelf != null)
            {
                _log.WarnFormat("Shelf '{0}' aleady exists. Cannot create.", name);
                throw new ArgumentException("Shelf already exists, cannot create a new one named: '{0}'".FormatWith(name));
            }

            AppDomainSetup settings = GetAppDomainSettings(name);

            AppDomain ad = AppDomain.CreateDomain(name, null, settings);

            // check the config for a bootstrapper if one isn't defined
            if (bootstrapper == null && File.Exists(settings.ConfigurationFile))
            {
                ShelfConfiguration config = ShelfConfiguration.GetConfig(settings.ConfigurationFile);
                if (config != null)
                {
                    bootstrapper = config.BootstrapperType;
                }
                else
                {
                    _log.ErrorFormat("Couldn't find a bootstrapper for shelf '{0}'", name);
                    throw new Exception("Couldn't find a bootstrapper for shelf '{0}'".FormatWith(name));
                }
            }

            assemblies.ToList().ForEach(x => ad.Load(x)); // add any missing assemblies
            Type shelfType = typeof(Shelf);
            ObjectHandle shelfHandle = ad.CreateInstance(shelfType.Assembly.GetName().FullName, shelfType.FullName, true, 0, null, new object[] { bootstrapper },
                                                         null, null);

            _shelves.WriteLock(dict => dict.Add(name, new ShelfHandle
                                   {
                                       AppDomain = ad,
                                       ObjectHandle = shelfHandle, //TODO: if this is never used do we need to keep a reference?
                                       ShelfChannelBuilder = appDomain => new WcfUntypedChannelProxy(new ThreadPoolFiber(), WellknownAddresses.GetShelfAddress(appDomain), "topshelf.me"),
                                       ShelfName = name
                                   }));
        }

        static AppDomainSetup GetAppDomainSettings(string name)
        {
            var settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";

            //TODO: Is this better?
            if (name == "TopShelf.DirectoryWatcher") return settings;

            settings.ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", name);
            settings.ConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", name, name + ".config");
            return settings;
        }

        public ShelfState GetState(string shelfName)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, shelfName));
            return shelf == null ? ShelfState.Unknown : shelf.CurrentState;
        }

        public void StartShelf(string shelfName)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, shelfName));
            if (shelf == null)
                return;

            shelf.ShelfChannel.Send(new StartService());
        }

        public void StopShelf(string shelfName)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, shelfName));
            if (shelf == null)
                return;

            shelf.ShelfChannel.Send(new StopService());
        }

        void MarkServiceReadyAndStart(ServiceReady message)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelf == null)
                return;

            ShelfState oldState = shelf.CurrentState;

            shelf.CurrentState = ShelfState.Ready;

            // if auto start is setup, send start
            shelf.ShelfChannel.Send(new StartService());

            StateChanged(oldState, ShelfState.Ready, message.ShelfName);
        }

        void MarkShelfReadyAndInitService(ShelfReady message)
        {
            ShelfHandle shelfStatus = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelfStatus == null)
                return;

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Readying;

            shelfStatus.ShelfChannel.Send(new ReadyService());

            StateChanged(oldState, ShelfState.Readying, message.ShelfName);
        }

        public delegate void ShelfStateChangedHandler(object sender, ShelfStateChangedEventArgs args);

        public event ShelfStateChangedHandler OnShelfStateChanged;

        void StateChanged(ShelfState oldSate, ShelfState newState, string shelfName)
        {
            ShelfStateChangedHandler handler = OnShelfStateChanged;
            if (handler != null)
            {
                handler(this, new ShelfStateChangedEventArgs { PreviousShelfState = oldSate, CurrentShelfState = newState, ShelfName = shelfName });
            }
        }

        public void Dispose()
        {
            if (_myChannelHost != null)
            {
                _myChannelHost.Dispose();
            }

            _shelves.WriteLock(dict => dict.Values.ToList().ForEach(x => AppDomain.Unload(x.AppDomain)));
        }
    }
}
