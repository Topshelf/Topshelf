// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf.Model
{
    using System;
    using System.Collections.Generic;

    public interface IOldServiceCoordinator :
        IDisposable
    {
        /// <summary>
        /// Starts all registered services, blocking the thread while they start
        /// </summary>
        void Start();
        /// <summary>
        /// Stops all registered services, blocking the thread while they stop
        /// </summary>
        void Stop();
        /// <summary>
        /// Pause all started services, blocking the thread while they stop
        /// </summary>
        void Pause();
        /// <summary>
        /// Continue all paused services, blocking the thread while they continue
        /// </summary>
        void Continue();

        /// <summary>
        /// Starts a given service, without blocking the current thread
        /// </summary>
        /// <param name="name">name of the service</param>
        void StartService(string name);
        /// <summary>
        /// Stops a given service, without blocking the current thread
        /// </summary>
        /// <param name="name">name of the service</param>
        void StopService(string name);
        /// <summary>
        /// Pauses a given service, without blocking the current thread
        /// </summary>
        /// <param name="name">name of the service</param>
        void PauseService(string name);
        /// <summary>
        /// Continues a given service, without blocking the current thread
        /// </summary>
        /// <param name="name">name of the service</param>
        void ContinueService(string name);

        /// <summary>
        /// The number of services currently hosted, mostly used for testing
        /// </summary>
        int HostedServiceCount { get; }
        /// <summary>
        /// Get the service controller for a given service, mostly used for testing
        /// </summary>
        /// <param name="s">name of the service</param>
        /// <returns>A service controller for the service in question</returns>
        IService GetService(string s);
        /// <summary>
        /// Provides a list of details about the hosted services
        /// </summary>
        /// <returns>List of ServiceInformation, describing the hosted services</returns>
        IList<ServiceInformation> GetServiceInfo();
    }
}