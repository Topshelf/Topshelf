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
    using ServiceConfigurators;

    public static class ServiceEventConfiguratorExtensions
    {
        /// <summary>
        /// Registers a callback invoked before the service Start method is called.
        /// </summary>
        public static T BeforeStartingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.BeforeStartingService(x => callback());

            return configurator;
        }

        /// <summary>
        /// Registers a callback invoked after the service Start method is called.
        /// </summary>
        public static T AfterStartingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AfterStartingService(x => callback());

            return configurator;
        }

        /// <summary>
        /// Registers a callback invoked before the service Stop method is called.
        /// </summary>
        public static T BeforeStoppingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.BeforeStoppingService(x => callback());

            return configurator;
        }

        /// <summary>
        /// Registers a callback invoked after the service Stop method is called.
        /// </summary>
        public static T AfterStoppingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AfterStoppingService(x => callback());

            return configurator;
        }
    }
}