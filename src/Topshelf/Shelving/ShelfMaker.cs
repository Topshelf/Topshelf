// Copyright 2007-2008 The Apache Software Foundation.
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
    using Magnum.Channels;
    using Magnum.Extensions;
    using Magnum.Fibers;
    using Magnum.Threading;
    using Messages;

    public class ShelfMaker :
        IDisposable
    {
        private readonly WcfUntypedChannelHost _myChannelHost;
		private readonly UntypedChannelAdapter _myChannel;
        private readonly ReaderWriterLockedObject<Dictionary<string, ShelfHandle>> _shelves;

        public ShelfMaker()
        {
            _shelves = new ReaderWriterLockedObject<Dictionary<string, ShelfHandle>>(new Dictionary<string, ShelfHandle>());

			_myChannel = new UntypedChannelAdapter(new ThreadPoolFiber());
			_myChannelHost = new WcfUntypedChannelHost(new SynchronousFiber(), _myChannel, WellknownAddresses.HostAddress, "topshelf.host");

            _myChannel.Subscribe(s =>
            {
                s.Consume<ShelfReady>().Using(m => MarkShelfReadyAndInitService(m));
                s.Consume<ServiceReady>().Using(m => MarkServiceReadyAndStart(m));
                s.Consume<ShelfStopped>().Using(m => MarkShelfStopped(m));
                s.Consume<ShelfStarted>().Using(m => MarkServiceStarted(m));
                s.Consume<FileSystemChange>().Using(m => ReloadShelf(m));
            });
        }

        private static ShelfHandle GetShelfStatus(Dictionary<string, ShelfHandle> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        private void ReloadShelf(FileSystemChange message)
        {
            ShelfHandle shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ServiceId));
            if (shelf != null)
            {
                if (shelf.CurrentState == ShelfState.Started)
                {
                    var resetEvent = shelf.StopHandle = shelf.StopHandle ?? new ManualResetEvent(false);

                    StopShelf(message.ServiceId);

                    resetEvent.WaitOne(30.Seconds());
                }

                AppDomain.Unload(shelf.AppDomain);
                _shelves.WriteLock(dict => dict.Remove(message.ServiceId));
            }

            MakeShelf(message.ServiceId);
        }

        private void MarkShelfStopped(ShelfStopped message)
        {
            var shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelf == null)
                throw new Exception("Shelf does not exist");

            if (shelf.StopHandle != null)
                shelf.StopHandle.Set();

            shelf.CurrentState = ShelfState.Stopped;
        }

        public void MakeShelf(string name, params AssemblyName[] assemblies)
        {
            MakeShelf(name, null, assemblies);
        }

        public void MakeShelf(string name, Type bootstrapper, params AssemblyName[] assemblies)
        {
            var shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, name));
            if (shelf != null)
                throw new ArgumentException("Shelf already exists, cannot create a new one named: " + name);

            AppDomainSetup settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";
            if (name != "TopShelf.DirectoryWatcher")
            {
                // uggh, don't like a specific exemption but I don't have a good solution to do otherwise yet
                settings.ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", name);
                settings.ConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", name, name + ".config");
            }
            AppDomain ad = AppDomain.CreateDomain(name, null, settings);
            
            // check the config for a bootstrapper if one isn't defined
            if (bootstrapper == null && File.Exists(settings.ConfigurationFile))
            {
                var config = ShelfConfiguration.GetConfig(settings.ConfigurationFile);
                if (config != null)
                {
                    bootstrapper = config.BootstrapperType;
                }
            }

            assemblies.ToList().ForEach(x => ad.Load(x)); // add any missing assemblies
            Type type = typeof(Shelf);
            ObjectHandle s = ad.CreateInstance(type.Assembly.GetName().FullName, type.FullName, true, 0, null, new object[] { bootstrapper },
                                               null, null);

            _shelves.WriteLock(dict => dict.Add(name, new ShelfHandle
                {
                    AppDomain = ad,
                    ObjectHandle = s,
                    ShelfChannelBuilder = appDomain => new WcfUntypedChannelProxy(new ThreadPoolFiber(), WellknownAddresses.GetShelfAddress(appDomain), "topshelf.me"),
                    ShelfName = name
                }));
        }

        public ShelfState GetState(string shelfName)
        {
            var shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, shelfName));
            return shelf == null ? ShelfState.Unknown : shelf.CurrentState;
        }

        public void StartShelf(string shelfName)
        {
            var shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, shelfName));
            if (shelf == null)
                throw new Exception("Shelf does not exist");

            shelf.ShelfChannel.Send(new StartService());
        }

        public void StopShelf(string shelfName)
        {
            var shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, shelfName));
            if (shelf == null)
                throw new Exception("Shelf does not exist");

            shelf.ShelfChannel.Send(new StopService());
        }

        private void MarkServiceReadyAndStart(ServiceReady message)
        {
            var shelf = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelf == null)
                throw new Exception("Shelf does not exist");

            var oldState = shelf.CurrentState;

            shelf.CurrentState = ShelfState.Ready;

            // if auto start is setup, send start
            shelf.ShelfChannel.Send(new StartService());

            StateChanged(oldState, ShelfState.Ready, message.ShelfName);
        }

        private void MarkShelfReadyAndInitService(ShelfReady message)
        {
            var shelfStatus = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelfStatus == null)
                throw new Exception("Shelf does not exist");

            var oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Readying;

            shelfStatus.ShelfChannel.Send(new ReadyService());

            StateChanged(oldState, ShelfState.Readying, message.ShelfName);
        }

        private void MarkServiceStarted(ShelfStarted message)
        {
            var shelfStatus = _shelves.UpgradeableReadLock(dict => GetShelfStatus(dict, message.ShelfName));
            if (shelfStatus == null)
                throw new Exception("Shelf does not exist");

            var oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Started;

            StateChanged(oldState, ShelfState.Started, message.ShelfName);
        }

        public delegate void ShelfStateChangedHandler(object sender, ShelfStateChangedEventArgs args);

        public event ShelfStateChangedHandler OnShelfStateChanged;

        private void StateChanged(ShelfState oldSate, ShelfState newState, string shelfName)
        {
            var handler = OnShelfStateChanged;
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