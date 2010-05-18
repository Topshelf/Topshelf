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
    using System.Linq;
    using Configuration.Dsl;
    using Magnum.Channels;
    using Magnum.Fibers;
    using Messages;
    using Model;

    public class Shelf :
        IDisposable
    {
        IServiceController _controller;
        WcfUntypedChannel _hostChannel;
        WcfUntypedChannelAdapter _myChannel;
        ChannelSubscription _subscription;

        public Shelf(Type bootstraper)
        {
            Initialize(bootstraper);
        }

        public void Initialize(Type bootstraper)
        {
            var t = FindBootstrapperImplementation(bootstraper);
            var b = (Bootstrapper)Activator.CreateInstance(t);

            var cfg = new ServiceConfigurator<object>();

            //have to do some type coearcion here
            //wonder if co/contra will help here?
            b.InitializeHostedService(cfg);

            //start up the service controller instance
            _controller = cfg.Create();

            //how do the addresses work (its a light wrapper over wcf)
            _hostChannel = new WcfUntypedChannel(new ThreadPoolFiber(), WellknownAddresses.HostAddress, "topshelf.host");
            _myChannel = new WcfUntypedChannelAdapter(new ThreadPoolFiber(), WellknownAddresses.CurrentShelfAddress, "topshelf.me");

            //wire up all the subscriptions
            _subscription = _myChannel.Subscribe(s =>
                                     {
                                         s.Consume<StopService>().Using(m => _controller.Stop());
                                         s.Consume<StartService>().Using(m => _controller.Start());
                                         s.Consume<PauseService>().Using(m => _controller.Pause());
                                         s.Consume<ContinueService>().Using(m => _controller.Continue());
                                     });

            //send message to host that I am ready
            _hostChannel.Send(new ShelfReady { ShelfName = AppDomain.CurrentDomain.FriendlyName });
        }

        static Type FindBootstrapperImplementation(Type bootstrapper)
        {
            if (bootstrapper != null)
            {
                if (bootstrapper.GetInterfaces().Where(x => x == typeof(Bootstrapper)).Count() > 0)
                    return bootstrapper;
                
                throw new InvalidOperationException("Bootstrapper type, " + bootstrapper.GetType().Name
                                                    + ", is not a subclass of Bootstrapper.");
            }

            var possibleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsInterface == false)
                .Where(x => typeof(Bootstrapper).IsAssignableFrom(x));

            if (possibleTypes.Count() > 1)
                throw new InvalidOperationException("Unable to identify the bootstrapper, more than one found.");

            if (possibleTypes.Count()  == 0)
                throw new InvalidOperationException("The bootstrapper was not found.");

            return possibleTypes.Single();
        }

        public void Dispose()
        {
            if (_subscription != null)
                _subscription.Dispose();

            if (_myChannel != null)
                _myChannel.Dispose();
        }
    }
}