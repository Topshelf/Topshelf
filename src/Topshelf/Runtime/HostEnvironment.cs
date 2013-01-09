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
namespace Topshelf.Runtime
{
    using System;

    /// <summary>
    /// Abstracts the environment in which the host in running (different OS versions, platforms,
    /// bitness, etc.)
    /// </summary>
    public interface HostEnvironment
    {
        string CommandLine { get; }

        /// <summary>
        /// Determines if the service is running as an administrator
        /// </summary>
        bool IsAdministrator { get; }

        /// <summary>
        /// Determines if the process is running as a service
        /// </summary>
        bool IsRunningAsAService { get; }

        /// <summary>
        /// Determines if the service is installed
        /// </summary>
        /// <param name="serviceName">The name of the service as it is registered</param>
        /// <returns>True if the service is installed, otherwise false</returns>
        bool IsServiceInstalled(string serviceName);

        /// <summary>
        /// Start the service using operating system controls
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        void StartService(string serviceName);

        /// <summary>
        /// Stop the service using operating system controls
        /// </summary>
        /// <param name="serviceName"></param>
        void StopService(string serviceName);

        /// <summary>
        /// Install the service using the settings provided
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="beforeInstall"> </param>
        /// <param name="afterInstall"> </param>
        /// <param name="beforeRollback"> </param>
        /// <param name="afterRollback"> </param>
        void InstallService(InstallHostSettings settings, Action beforeInstall, Action afterInstall, Action beforeRollback, Action afterRollback);

        /// <summary>
        /// Uninstall the service using the settings provided
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="beforeUninstall"></param>
        /// <param name="afterUninstall"></param>
        void UninstallService(HostSettings settings, Action beforeUninstall, Action afterUninstall);

        /// <summary>
        /// Restarts the service as an administrator which has permission to modify the service configuration
        /// </summary>
        /// <returns>True if the child process was executed, otherwise false</returns>
        bool RunAsAdministrator();

        /// <summary>
        /// Create a service host appropriate for the host environment
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="serviceHandle"></param>
        /// <returns></returns>
        Host CreateServiceHost(HostSettings settings, ServiceHandle serviceHandle);
    }
}