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
namespace Topshelf.ServiceConfigurators
{
    using System;
    using Runtime;
    using System.ServiceProcess;

    public interface ServiceConfigurator
    {
        /// <summary>
        /// Registers a callback invoked before the service Start method is called.
        /// </summary>
        void BeforeStartingService(Action<HostStartContext> callback);

        /// <summary>
        /// Registers a callback invoked after the service Start method is called.
        /// </summary>
        void AfterStartingService(Action<HostStartedContext> callback);

        /// <summary>
        /// Registers a callback invoked before the service Stop method is called.
        /// </summary>
        void BeforeStoppingService(Action<HostStopContext> callback);

        /// <summary>
        /// Registers a callback invoked after the service Stop method is called.
        /// </summary>
        void AfterStoppingService(Action<HostStoppedContext> callback);
    }

    public interface ServiceConfigurator<T> :
        ServiceConfigurator
        where T : class
    {
        void ConstructUsing(ServiceFactory<T> serviceFactory);
        void WhenStarted(Func<T, HostControl, bool> start);
        void WhenStopped(Func<T, HostControl, bool> stop);
        void WhenPaused(Func<T, HostControl, bool> pause);
        void WhenContinued(Func<T, HostControl, bool> @continue);
        void WhenShutdown(Action<T, HostControl> shutdown);
        void WhenSessionEvent(Action<T, HostControl, SessionChangeReason, int> sessionevent);
    }
}