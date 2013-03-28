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
namespace Topshelf.Builders
{
    using System;
    using Runtime;
    using System.ServiceProcess;

    public class ControlServiceBuilder<T> :
        ServiceBuilder
        where T : class, ServiceControl
    {
        readonly ServiceEvents _serviceEvents;
        readonly Func<HostSettings, T> _serviceFactory;

        public ControlServiceBuilder(Func<HostSettings, T> serviceFactory, ServiceEvents serviceEvents)
        {
            _serviceFactory = serviceFactory;
            _serviceEvents = serviceEvents;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                T service = _serviceFactory(settings);

                return new ControlServiceHandle(service, _serviceEvents);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        class ControlServiceHandle :
            ServiceHandle
        {
            readonly T _service;
            readonly ServiceEvents _serviceEvents;

            public ControlServiceHandle(T service, ServiceEvents serviceEvents)
            {
                _service = service;
                _serviceEvents = serviceEvents;
            }

            public void Dispose()
            {
                var disposable = _service as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            public bool Start(HostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);
                bool started = _service.Start(hostControl);
                if (started)
                    _serviceEvents.AfterStart(hostControl);
                return started;
            }

            public bool Stop(HostControl hostControl)
            {
                _serviceEvents.BeforeStop(hostControl);
                bool stopped = _service.Stop(hostControl);
                if (stopped)
                    _serviceEvents.AfterStop(hostControl);
                return stopped;
            }

            public bool Pause(HostControl hostControl)
            {
                var service = _service as ServiceSuspend;

                return service != null && service.Pause(hostControl);
            }

            public bool Continue(HostControl hostControl)
            {
                var service = _service as ServiceSuspend;

                return service != null && service.Continue(hostControl);
            }

            public void Shutdown(HostControl hostControl)
            {
                var serviceShutdown = _service as ServiceShutdown;
                if (serviceShutdown != null)
                {
                    serviceShutdown.Shutdown(hostControl);
                }
            }

            public void SessionEvent(HostControl hostControl, SessionChangeReason reason, int id)
            {
                // nothing to control
            }
        }
    }
}