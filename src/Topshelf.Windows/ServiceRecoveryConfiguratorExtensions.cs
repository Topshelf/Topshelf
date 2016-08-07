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

    public static class ServiceRecoveryConfiguratorExtensions
    {
        public static HostConfigurator EnableServiceRecovery(this HostConfigurator configurator, Action<ServiceRecoveryConfigurator> configureCallback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");
            if (configureCallback == null)
                throw new ArgumentNullException("configureCallback");

            var recoveryHostConfigurator = new ServiceRecoveryHostConfigurator();

            configureCallback(recoveryHostConfigurator);

            configurator.AddConfigurator(recoveryHostConfigurator);

            return configurator;
        }
    }
}