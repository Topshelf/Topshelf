// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf
{
    using System;
    using HostConfigurators;
    using Runtime;
    using ServiceConfigurators;
    using Supervise;

    public static class SuperviseServiceExtensions
    {
        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator)
            where T : class, ServiceControl, new()
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = ServiceExtensions.CreateServiceBuilderFactory(x => new T(),
                NoCallback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator,
            Action<ServiceConfigurator> callback)
            where T : class, ServiceControl, new()
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = ServiceExtensions.CreateServiceBuilderFactory(x => new T(),
                callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator, Func<T> serviceFactory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory =
                ServiceExtensions.CreateServiceBuilderFactory(x => serviceFactory(), NoCallback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator, Func<T> serviceFactory,
            Action<ServiceConfigurator> callback)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory =
                ServiceExtensions.CreateServiceBuilderFactory(x => serviceFactory(), callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator,
            Func<HostSettings, T> serviceFactory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory =
                ServiceExtensions.CreateServiceBuilderFactory(serviceFactory, NoCallback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator,
            Func<HostSettings, T> serviceFactory, Action<ServiceConfigurator> callback)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory =
                ServiceExtensions.CreateServiceBuilderFactory(serviceFactory, callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static HostConfigurator Service<T>(this HostConfigurator configurator,
            ServiceBuilderFactory serviceBuilderFactory)
            where T : class
        {
            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }


        public static SuperviseConfigurator Service<TService>(this SuperviseConfigurator configurator,
            Action<ServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = ServiceExtensions.CreateServiceBuilderFactory(callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        static void NoCallback(ServiceConfigurator configurator)
        {
        }
    }
}