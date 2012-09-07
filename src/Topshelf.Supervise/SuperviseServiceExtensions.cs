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
    using System.Linq;
    using Builders;
    using Configurators;
    using HostConfigurators;
    using Runtime;
    using ServiceConfigurators;

    public static class SuperviseServiceExtensions
    {
        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator)
            where T : class, ServiceControl, new()
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory factory = settings => new ControlServiceBuilder<T>(x => new T());

            configurator.UseServiceBuilder(factory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator, Func<T> serviceFactory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory factory = settings => new ControlServiceBuilder<T>(x => serviceFactory());

            configurator.UseServiceBuilder(factory);

            return configurator;
        }

        public static SuperviseConfigurator Service<T>(this SuperviseConfigurator configurator,
            Func<HostSettings, T> serviceFactory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory factory = settings => new ControlServiceBuilder<T>(x => serviceFactory(x));

            configurator.UseServiceBuilder(factory);

            return configurator;
        }


        public static SuperviseConfigurator Service<TService>(this SuperviseConfigurator configurator,
            Action<ServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");
            if (callback == null)
                throw new ArgumentNullException("callback");

            var serviceConfigurator = new DelegateServiceConfigurator<TService>();

            callback(serviceConfigurator);

            configurator.UseServiceBuilder(x =>
                {
                    ConfigurationResult configurationResult =
                        ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                    if (configurationResult.Results.Any())
                        throw new HostConfigurationException("The service was not properly configured");

                    ServiceBuilder serviceBuilder = serviceConfigurator.Build();

                    return serviceBuilder;
                });

            return configurator;
        }
    }
}