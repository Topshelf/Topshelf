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

    public abstract class ServiceConfiguratorBase
    {
        protected readonly ServiceEventsImpl ServiceEvents;

        protected ServiceConfiguratorBase()
        {
            ServiceEvents = new ServiceEventsImpl();
        }

        public void BeforeStartingService(Action<HostStartContext> callback)
        {
            ServiceEvents.AddBeforeStart(callback);
        }

        public void AfterStartingService(Action<HostStartedContext> callback)
        {
            ServiceEvents.AddAfterStart(callback);
        }

        public void BeforeStoppingService(Action<HostStopContext> callback)
        {
            ServiceEvents.AddBeforeStop(callback);
        }

        public void AfterStoppingService(Action<HostStoppedContext> callback)
        {
            ServiceEvents.AddAfterStop(callback);
        }
    }
}