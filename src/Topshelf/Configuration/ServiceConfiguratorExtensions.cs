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
    using System.ServiceProcess;

    public static class ServiceConfiguratorExtensions
    {
        public static ServiceConfigurator<T> ConstructUsing<T>(this ServiceConfigurator<T> configurator, Func<T> factory)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.ConstructUsing(settings => factory());

            return configurator;
        }

        public static ServiceConfigurator<T> ConstructUsing<T>(this ServiceConfigurator<T> configurator,
            Func<string, T> factory)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.ConstructUsing(settings => factory(typeof(T).Name));

            return configurator;
        }

        public static ServiceConfigurator<T> WhenStarted<T>(this ServiceConfigurator<T> configurator, Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenStarted((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenStopped<T>(this ServiceConfigurator<T> configurator, Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenStopped((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenPaused<T>(this ServiceConfigurator<T> configurator, Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenPaused((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenContinued<T>(this ServiceConfigurator<T> configurator,
            Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenContinued((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenShutdown<T>(this ServiceConfigurator<T> configurator,
            Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenShutdown((service, control) =>
                {
                    callback(service);
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenSessionEvent<T>(this ServiceConfigurator<T> configurator,
            Action<T, SessionChangeReason, int> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenSessionEvent((service, control, reason, id) =>
            {
                callback(service, reason, id);
            });

            return configurator;
        }
    }
}