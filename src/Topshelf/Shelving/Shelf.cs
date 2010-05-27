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
    using Magnum.Reflection;
    using Messages;
    using Model;

    public class Shelf :
        IDisposable
    {
        private IServiceController _controller;
        private readonly WcfUntypedChannel _hostChannel;
        private readonly WcfUntypedChannelAdapter _myChannel;
        private readonly ChannelSubscription _subscription;
        private readonly Type _bootstrapperType;

        public Shelf(Type bootstraper)
        {
            _bootstrapperType = bootstraper;
            _hostChannel = new WcfUntypedChannel(new ThreadPoolFiber(), WellknownAddresses.HostAddress, "topshelf.host");
            _myChannel = new WcfUntypedChannelAdapter(new ThreadPoolFiber(), WellknownAddresses.CurrentShelfAddress, "topshelf.me");

            //wire up all the subscriptions
            _subscription = _myChannel.Subscribe(s =>
                                     {
                                         s.Consume<ReadyService>().Using(m => Initialize());
                                         s.Consume<StopService>().Using(m => HandleStop(m));
                                         s.Consume<StartService>().Using(m => HandleStart(m));
                                         s.Consume<PauseService>().Using(m => _controller.Pause());
                                         s.Consume<ContinueService>().Using(m => _controller.Continue());
                                     });

            //send message to host that I am ready
            _hostChannel.Send(new ShelfReady());
        }

        public void Initialize()
        {
            var t = FindBootstrapperImplementation(_bootstrapperType);
            var bs = Activator.CreateInstance(t);

            //TODO: issue is here
            var st = bs.GetType().GetInterfaces()[0].GetGenericArguments()[0];
            var cfg = FastActivator.Create(typeof(ServiceConfigurator<>).MakeGenericType(st));

            this.FastInvoke(new[] { st }, "InitializeAndCreateHostedService", bs, cfg);

            _hostChannel.Send(new ServiceReady());
        }

        private void InitializeAndCreateHostedService<T>(Bootstrapper<T> bootstrapper, ServiceConfigurator<T> cfg)
            where T : class
        {
            bootstrapper.FastInvoke("InitializeHostedService", cfg);
            //start up the service controller instance
            _controller = cfg.FastInvoke<ServiceConfigurator<T>, IServiceController>("Create");
        }


        public static Type FindBootstrapperImplementation(Type bootstrapper)
        {
            if (bootstrapper != null)
            {
                if (bootstrapper.GetInterfaces().Where(IsBootstrapperType).Count() > 0)
                    return bootstrapper;

                throw new InvalidOperationException("Bootstrapper type, " + bootstrapper.GetType().Name
                                                    + ", is not a subclass of Bootstrapper.");
            }

            var possibleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsInterface == false)
                .Where(t => t.GetInterfaces().Any(IsBootstrapperType));

            if (possibleTypes.Count() > 1)
                throw new InvalidOperationException("Unable to identify the bootstrapper, more than one found.");

            if (possibleTypes.Count() == 0)
                throw new InvalidOperationException("The bootstrapper was not found.");

            return possibleTypes.Single();
        }

        private static bool IsBootstrapperType(Type t)
        {
            if (t.IsGenericType)
            {
                return t.GetGenericTypeDefinition() == typeof(Bootstrapper<>);
            }
            return false;
        }

        private void HandleStart(StartService message)
        {
            _hostChannel.Send(new ShelfStarting());
            _controller.Start();
            _hostChannel.Send(new ShelfStarted());
        }

        private void HandleStop(StopService message)
        {
            _hostChannel.Send(new ShelfStopping());
            _controller.Stop();
            _hostChannel.Send(new ShelfStopped());
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