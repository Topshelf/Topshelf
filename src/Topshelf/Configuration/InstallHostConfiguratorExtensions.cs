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

    public static class InstallHostConfiguratorExtensions
    {
        public static HostConfigurator BeforeInstall(this HostConfigurator configurator, Action callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeInstall",
                x => x.BeforeInstall(settings => callback())));

            return configurator;
        }

        public static HostConfigurator BeforeInstall(this HostConfigurator configurator,
            Action<InstallHostSettings> callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeInstall",
                x => x.BeforeInstall(callback)));

            return configurator;
        }

        public static HostConfigurator AfterInstall(this HostConfigurator configurator, Action callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterInstall",
                x => x.AfterInstall(settings => callback())));

            return configurator;
        }

        public static HostConfigurator AfterInstall(this HostConfigurator configurator,
            Action<InstallHostSettings> callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterInstall", x => x.AfterInstall(callback)));

            return configurator;
        }
    }
}