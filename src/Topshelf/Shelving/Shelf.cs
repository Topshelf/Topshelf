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
        IServiceController _controller;
		readonly UntypedChannel _hostChannel;
		readonly WcfChannelHost _myChannelHost;
		readonly ChannelAdapter _myChannel;
        readonly ChannelConnection _connection;
        readonly Type _bootstrapperType;

        public Shelf(Type bootstraper)
        {
            _bootstrapperType = bootstraper;
            _hostChannel = WellknownAddresses.GetHostChannelProxy();
			_myChannel = new ChannelAdapter();
            _myChannelHost = WellknownAddresses.GetCurrentShelfHost(_myChannel);

            //wire up all the subscriptions
            _connection = _myChannel.Connect(s =>
                                     {
                                         s.Consume<ReadyService>().Using(m => Initialize());
                                         s.Consume<StopService>().Using(m => HandleStop(m));
                                         s.Consume<StartService>().Using(m => HandleStart(m));
                                         s.Consume<PauseService>().Using(m => HandlePause(m));
                                         s.Consume<ContinueService>().Using(m => HandleContinue(m));
                                     });

            //send message to host that I am ready
            _hostChannel.Send(new ShelfReady());
        }

        public void Initialize()
        {
            try
            {
                Type t = FindBootstrapperImplementation(_bootstrapperType);
                object bs = Activator.CreateInstance(t);

                Type st = bs.GetType().GetInterfaces()[0].GetGenericArguments()[0];
                object cfg = FastActivator.Create(typeof(ServiceConfigurator<>).MakeGenericType(st));

                this.FastInvoke(new[] { st }, "InitializeAndCreateHostedService", bs, cfg);

                _hostChannel.Send(new ServiceReady());
            }
            catch (Exception ex)
            {
                SendFault(ex);
            }

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

                throw new InvalidOperationException("Bootstrapper type, '{0}', is not a subclass of Bootstrapper.".FormatWith(bootstrapper.GetType().Name));
            }
            
            // check configuration first
            ShelfConfiguration config = ShelfConfiguration.GetConfig();
            if (config != null)
            {
                return config.BootstrapperType;
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
            try
            {
                _hostChannel.Send(new ServiceStarting());
                _controller.Start();
                _hostChannel.Send(new ServiceStarted());
            }
            catch (Exception ex)
            {
                SendFault(ex);
            }
        }

        private void HandleStop(StopService message)
        {
            try
            {
                _hostChannel.Send(new ServiceStopping());
                _controller.Stop();
                _hostChannel.Send(new ServiceStopped());
            }
            catch (Exception ex)
            {
                SendFault(ex);
            }
        }

        private void HandlePause(PauseService messsage)
        {
            try
            {
                _hostChannel.Send(new ServicePausing());
                _controller.Pause();
                _hostChannel.Send(new ServicePaused());
            }
            catch (Exception ex)
            {
                SendFault(ex);
            }
        }

        private void HandleContinue(ContinueService message)
        {
            try
            {
                _hostChannel.Send(new ServiceContinuing());
                _controller.Continue();
                _hostChannel.Send(new ServiceContinued());
            }
            catch (Exception ex)
            {
                SendFault(ex);
            }
        }


        private void SendFault(Exception exception)
        {
            try
            {
                _hostChannel.Send(new ShelfFault(exception));
            }
            catch (Exception)
            {
                // eat the exception for now
            }
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();

            if (_myChannelHost != null)
                _myChannelHost.Dispose();
        }
    }
}