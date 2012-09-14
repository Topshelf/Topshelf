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
    using Rehab;
    using Runtime;
    using ServiceConfigurators;

    public static class RehabServiceExtensions
    {
        public static HostConfigurator RehabService<T>(this HostConfigurator configurator)
            where T : class, ServiceControl, new()
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = ServiceExtensions.CreateServiceBuilderFactory(x => new T(),
                NoCallback);

            return RehabService<T>(configurator, serviceBuilderFactory);
        }

        public static HostConfigurator RehabService<T>(this HostConfigurator configurator, Func<T> factory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = ServiceExtensions.CreateServiceBuilderFactory(x => factory(),
                NoCallback);

            return RehabService<T>(configurator, serviceBuilderFactory);
        }

        public static HostConfigurator RehabService<T>(this HostConfigurator configurator, Func<HostSettings, T> factory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = ServiceExtensions.CreateServiceBuilderFactory(factory,
                NoCallback);

            return RehabService<T>(configurator, serviceBuilderFactory);
        }

        public static HostConfigurator RehabService<T>(this HostConfigurator configurator,
            ServiceBuilderFactory serviceBuilderFactory)
            where T : class
        {
            ServiceBuilderFactory rehabFactory = settings => new RehabServiceBuilder<T>(serviceBuilderFactory);

            configurator.UseServiceBuilder(rehabFactory);

            return configurator;
        }

        static void NoCallback(ServiceConfigurator configurator)
        {
        }
    }
}