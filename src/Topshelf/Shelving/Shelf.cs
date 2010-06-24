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
    using log4net;
    using Magnum.Channels;
    using Magnum.Reflection;
    using Messages;
    using Model;

    public class Shelf :
        IDisposable
    {
        static ILog _log = LogManager.GetLogger(typeof(Shelf));
        IServiceController _controller;
        readonly Type _bootstrapperType;
        readonly UntypedChannel _hostChannel;

        public Shelf(Type bootstraper)
        {
            _hostChannel = WellknownAddresses.GetHostChannelProxy();
            _bootstrapperType = bootstraper;

            Initialize();
        }

        void Initialize()
        {
            try
            {
                var t = FindBootstrapperImplementationType(_bootstrapperType);
                var bootstrapper = Activator.CreateInstance(t);

                var serviceType = bootstrapper.GetType().GetInterfaces()[0].GetGenericArguments()[0];
                var cfg = FastActivator.Create(typeof(ServiceConfigurator<>).MakeGenericType(serviceType));


                this.FastInvoke(new[] { serviceType }, "InitializeAndCreateHostedService", bootstrapper, cfg);

                _controller.Initialize();
            }
            catch (Exception ex)
            {
                SendFault(ex);
            }

        }
        
        //Converts the cfg to the closed type from the object type
        private void InitializeAndCreateHostedService<T>(Bootstrapper<T> bootstrapper, ServiceConfigurator<T> cfg)
            where T : class
        {
            bootstrapper.FastInvoke("InitializeHostedService", cfg);
           
            _controller = cfg.Create();
        }

        public static Type FindBootstrapperImplementationType(Type bootstrapper)
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


        public string Name
        {
            get
            {
                return AppDomain.CurrentDomain.FriendlyName;
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
                _log.Error("Shelf '{0}' is having a bad day.", exception);
                // eat the exception for now
            }
        }

        public void Dispose()
        {
            //TODO: what 
        }
    }
}