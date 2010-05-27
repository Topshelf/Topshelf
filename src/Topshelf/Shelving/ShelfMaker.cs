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
    using Magnum.Channels;
    using Magnum.Fibers;
    using Messages;

    public class ShelfMaker :
        IDisposable
    {
        private readonly WcfUntypedChannelAdapter _myChannel;
        private readonly Dictionary<string, ShelfStatus> _shelves;

        public ShelfMaker()
        {
            _shelves = new Dictionary<string, ShelfStatus>();

            _myChannel = new WcfUntypedChannelAdapter(new SynchronousFiber(), WellknownAddresses.HostAddress, "topshelf.host");

            _myChannel.Subscribe(s =>
            {
                s.Consume<ShelfReady>().Using(m => MarkShelfReadyAndInitService(m));
                s.Consume<ServiceReady>().Using(m => MarkServiceReadyAndStart(m));
                s.Consume<ShelfStopped>().Using(m => MarkShelfStopped(m));
                s.Consume<ShelfStarted>().Using(m => MarkServiceStarted(m));
            });
        }

        private void MarkShelfStopped(ShelfStopped message)
        {
            if (!_shelves.ContainsKey(message.ShelfName))
                throw new Exception("Shelf does not exist");

            var shelfStatus = _shelves[message.ShelfName];

            shelfStatus.CurrentState = ShelfState.Stopped;
        }

        public void MakeShelf(string name, params AssemblyName[] assemblies)
        {
            MakeShelf(name, null, assemblies);
        }

        public void MakeShelf(string name, Type bootstrapper, params AssemblyName[] assemblies)
        {
            if (_shelves.ContainsKey(name))
                throw new ArgumentException("Shelf already exists, cannot create a new one named: " + name);

            AppDomainSetup settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";
            if (name != "TopShelf.DirectoryWatcher")
            {
                // uggh, shouldn't have to do this... revisit to fixy
                settings.ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", name);
                settings.ConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", name, name + ".config");
            }
            AppDomain ad = AppDomain.CreateDomain(name, null, settings);
            // should we query the service.config to look for any additional assemblies 
            // or anything else before we start the system?
            assemblies.ToList().ForEach(x => ad.Load(x)); // add any missing assemblies
            Type type = typeof(Shelf);
            ObjectHandle s = ad.CreateInstance(type.Assembly.GetName().FullName, type.FullName, true, 0, null, new object[] { bootstrapper },
                                               null, null);

            _shelves.Add(name, new ShelfStatus
                {
                    AppDomain = ad,
                    ObjectHandle = s,
                    ShelfChannelBuilder = appDomain => new WcfUntypedChannel(new ThreadPoolFiber(), WellknownAddresses.GetShelfAddress(appDomain), "topshelf.me"),
                    ShelfName = name
                });
        }

        public ShelfState GetState(string shelfName)
        {
            return _shelves[shelfName].CurrentState;
        }

        public void StartShelf(string shelfName)
        {
            if (!_shelves.ContainsKey(shelfName))
                throw new Exception("Shelf does not exist");

            _shelves[shelfName].ShelfChannel.Send(new StartService());
        }

        public void StopShelf(string shelfName)
        {
            if (!_shelves.ContainsKey(shelfName))
                throw new Exception("Shelf does not exist");

            _shelves[shelfName].ShelfChannel.Send(new StopService());
        }

        private void MarkServiceReadyAndStart(ServiceReady message)
        {
            if (!_shelves.ContainsKey(message.ShelfName))
                throw new Exception("Shelf does not exist");

            var shelfStatus = _shelves[message.ShelfName];

            var oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Ready;

            // if auto start is setup, send start
            shelfStatus.ShelfChannel.Send(new StartService());

            StateChanged(oldState, ShelfState.Ready, message.ShelfName);
        }

        private void MarkShelfReadyAndInitService(ShelfReady message)
        {
            if (!_shelves.ContainsKey(message.ShelfName))
                throw new Exception("Shelf does not exist");

            var shelfStatus = _shelves[message.ShelfName];

            var oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Readying;

            shelfStatus.ShelfChannel.Send(new ReadyService());

            StateChanged(oldState, ShelfState.Readying, message.ShelfName);
        }

        private void MarkServiceStarted(ShelfStarted message)
        {
            if (!_shelves.ContainsKey(message.ShelfName))
                throw new Exception("Shelf does not exist");

            var shelfStatus = _shelves[message.ShelfName];

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
            if (_myChannel != null)
            {
                _myChannel.Dispose();
            }

            _shelves.Values.ToList().ForEach(x => AppDomain.Unload(x.AppDomain));
        }
    }
}