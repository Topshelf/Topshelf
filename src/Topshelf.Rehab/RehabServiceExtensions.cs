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
    using Builders;
    using HostConfigurators;
    using Rehab;
    using Runtime;

    public static class RehabServiceExtensions
    {
        public static HostConfigurator RehabService<T>(this HostConfigurator configurator)
            where T : class, ServiceControl, new()
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory factory = settings => new ControlServiceBuilder<T>(x => new T());

            ServiceBuilderFactory updaterFactory = settings => new RehabServiceBuilder<T>(factory);

            configurator.UseServiceBuilder(updaterFactory);

            return configurator;
        }

        public static HostConfigurator RehabService<T>(this HostConfigurator configurator, Func<T> serviceFactory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory factory = settings => new ControlServiceBuilder<T>(x => serviceFactory());

            ServiceBuilderFactory updaterFactory = settings => new RehabServiceBuilder<T>(factory);

            configurator.UseServiceBuilder(updaterFactory);

            return configurator;
        }

        public static HostConfigurator RehabService<T>(this HostConfigurator configurator,
            Func<HostSettings, T> serviceFactory)
            where T : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory factory = settings => new ControlServiceBuilder<T>(x => serviceFactory(x));

            ServiceBuilderFactory updaterFactory = settings => new RehabServiceBuilder<T>(factory);

            configurator.UseServiceBuilder(updaterFactory);

            return configurator;
        }
    }
}