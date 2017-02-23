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
    using System.ServiceProcess;
    using HostConfigurators;

    public static class RunAsExtensions
    {
        public static HostConfigurator RunAs(this HostConfigurator configurator, string username, string password)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsUserHostConfigurator(username, password);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsVirtualServiceAccount(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsVirtualAccountHostConfigurator();

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsPrompt(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.User);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsNetworkService(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.NetworkService);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsLocalSystem(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.LocalSystem);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsLocalService(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.LocalService);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }
    }
}