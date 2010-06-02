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
    using Magnum.Channels;
    using Magnum.Extensions;
    using Magnum.Fibers;
    using Messages;

    public class ShelfMaker :
        IDisposable
    {
        readonly WcfUntypedChannelHost _myChannelHost;
        readonly UntypedChannelAdapter _myChannel;
        readonly Dictionary<string, ShelfInformation> _shelves;

        public ShelfMaker()
        {
            _shelves = new Dictionary<string, ShelfInformation>();


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

        void ReloadShelf(FileSystemChange message)
        {
            if (_shelves.ContainsKey(message.ServiceId))
            {
                ShelfInformation shelf = _shelves[message.ServiceId];
                ManualResetEvent resetEvent = shelf.StopHandle = shelf.StopHandle ?? new ManualResetEvent(false);

                StopShelf(message.ServiceId);

                resetEvent.WaitOne(30.Seconds());

                AppDomain.Unload(shelf.AppDomain);
                _shelves.Remove(message.ServiceId);
            }

            MakeShelf(message.ServiceId);
        }

        void MarkShelfStopped(ShelfStopped message)
        {
            if (!_shelves.ContainsKey(message.ShelfName))
                throw new Exception("Shelf does not exist");

            ShelfInformation shelfStatus = _shelves[message.ShelfName];

            if (shelfStatus.StopHandle != null)
                shelfStatus.StopHandle.Set();

            shelfStatus.CurrentState = ShelfState.Stopped;
        }

        public void MakeShelf(string name, params AssemblyName[] assemblies)
        {
            MakeShelf(name, null, assemblies);
        }

        public void MakeShelf(string name, Type bootstrapper, params AssemblyName[] assemblies)
        {
            if (_shelves.ContainsKey(name))
                throw new ArgumentException("Shelf already exists, cannot create a new one named: '{0}'".FormatWith(name));
            //TODO: Is this an exception? or should we just exit and log it?

            var settings = GetAppDomainSettings(name);
            var ad = AppDomain.CreateDomain(name, null, settings);

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
                    throw new Exception("Couldn't find a bootstrapper");
                }
            }

            assemblies.ToList().ForEach(x => ad.Load(x)); // add any missing assemblies
            Type shelfType = typeof (Shelf);
            ObjectHandle shelfHandle = ad.CreateInstance(shelfType.Assembly.GetName().FullName, shelfType.FullName, true, 0, null, new object[] {bootstrapper},
                                                         null, null);

            _shelves.Add(name, new ShelfInformation
                                   {
                                       AppDomain = ad,
                                       ObjectHandle = shelfHandle, //TODO: if this is never used do we need to keep a reference?
                                       ShelfChannelBuilder = appDomain => new WcfUntypedChannelProxy(new ThreadPoolFiber(), WellknownAddresses.GetShelfAddress(appDomain), "topshelf.me"),
                                       ShelfName = name
                                   });
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
            return !_shelves.ContainsKey(shelfName) ? ShelfState.Unknown : _shelves[shelfName].CurrentState;
        }

        public void StartShelf(string shelfName)
        {
            if (!_shelves.ContainsKey(shelfName))
                throw new Exception("Shelf does not exist"); //TODO: Is this really an exception?

            _shelves[shelfName].ShelfChannel.Send(new StartService());
        }

        public void StopShelf(string shelfName)
        {
            if (!_shelves.ContainsKey(shelfName)) //TODO: Is this really an exception?
                throw new Exception("Shelf does not exist");

            _shelves[shelfName].ShelfChannel.Send(new StopService());
        }

        void MarkServiceReadyAndStart(ServiceReady message)
        {
            if (!_shelves.ContainsKey(message.ShelfName)) //TODO: Is this really an exception?
                throw new Exception("Shelf does not exist");

            ShelfInformation shelfStatus = _shelves[message.ShelfName];

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Ready;

            // if auto start is setup, send start
            shelfStatus.ShelfChannel.Send(new StartService());

            StateChanged(oldState, ShelfState.Ready, message.ShelfName);
        }

        void MarkShelfReadyAndInitService(ShelfReady message)
        {
            if (!_shelves.ContainsKey(message.ShelfName)) //TODO: Is this really an exception?
                throw new Exception("Shelf does not exist");

            ShelfInformation shelfStatus = _shelves[message.ShelfName];

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Readying;

            shelfStatus.ShelfChannel.Send(new ReadyService());

            StateChanged(oldState, ShelfState.Readying, message.ShelfName);
        }

        void MarkServiceStarted(ShelfStarted message)
        {
            if (!_shelves.ContainsKey(message.ShelfName)) //TODO: Is this really an exception?
                throw new Exception("Shelf does not exist");

            ShelfInformation shelfStatus = _shelves[message.ShelfName];

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Started;

            StateChanged(oldState, ShelfState.Started, message.ShelfName);
        }

        public delegate void ShelfStateChangedHandler(object sender, ShelfStateChangedEventArgs args);

        public event ShelfStateChangedHandler OnShelfStateChanged;

        void StateChanged(ShelfState oldSate, ShelfState newState, string shelfName)
        {
            ShelfStateChangedHandler handler = OnShelfStateChanged;
            if (handler != null)
            {
                handler(this, new ShelfStateChangedEventArgs {PreviousShelfState = oldSate, CurrentShelfState = newState, ShelfName = shelfName});
            }
        }

        public void Dispose()
        {
            if (_myChannelHost != null)
            {
                _myChannelHost.Dispose();
            }

            _shelves.Values.ToList().ForEach(x => AppDomain.Unload(x.AppDomain));
        }
    }
}