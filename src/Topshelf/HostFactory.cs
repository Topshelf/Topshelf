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
    using Configurators;
    using HostConfigurators;
    using Logging;

    public static class HostFactory
    {
        static readonly Log _log = Logger.Get(typeof(HostFactory));

        public static Host New(Action<HostConfigurator> configureCallback)
        {
            if (configureCallback == null)
                throw new ArgumentNullException("configureCallback");

            var configurator = new HostConfiguratorImpl();

            Type declaringType = configureCallback.Method.DeclaringType;
            if (declaringType != null)
            {
                string defaultServiceName = declaringType.Namespace;
                if (!string.IsNullOrEmpty(defaultServiceName))
                    configurator.SetServiceName(defaultServiceName);
            }

            configureCallback(configurator);

            configurator.ApplyCommandLine();

            ConfigurationResult result = ValidateConfigurationResult.CompileResults(configurator.Validate());

            if (result.Message.Length > 0)
                _log.InfoFormat("Configuration Result:\n{0}", result.Message);

            return configurator.CreateHost();
        }

        public static void Run(Action<HostConfigurator> configure)
        {
            try
            {
                New(configure)
                    .Run();
            }
            catch (Exception ex)
            {
                _log.Error("The service terminated abnormally", ex);
            }
        }
    }
}