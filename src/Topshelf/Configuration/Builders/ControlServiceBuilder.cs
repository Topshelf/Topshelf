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

    public class ControlServiceBuilder<T> :
        ServiceBuilder
        where T : class, ServiceControl
    {
        readonly ServiceFactory<T> _serviceFactory;

        public ControlServiceBuilder(ServiceFactory<T> serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                T service = _serviceFactory(settings);

                return new ControlServiceHandle(service);
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

            public ControlServiceHandle(T service)
            {
                _service = service;
            }

            public void Dispose()
            {
                var disposable = _service as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            public bool Start(HostControl hostControl)
            {
                return _service.Start(hostControl);
            }

            public bool Stop(HostControl hostControl)
            {
                return _service.Stop(hostControl);
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
        }
    }
}