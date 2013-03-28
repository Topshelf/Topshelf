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

    public class DelegateServiceBuilder<T> :
        ServiceBuilder
        where T : class
    {
        readonly Func<T, HostControl, bool> _continue;
        readonly Func<T, HostControl, bool> _pause;
        readonly ServiceEvents _serviceEvents;
        readonly ServiceFactory<T> _serviceFactory;
        readonly Action<T, HostControl> _shutdown;
        readonly Func<T, HostControl, bool> _start;
        readonly Func<T, HostControl, bool> _stop;
        readonly Action<T, HostControl, SessionChangeReason, int> _sessionevent;

        public DelegateServiceBuilder(ServiceFactory<T> serviceFactory, Func<T, HostControl, bool> start,
            Func<T, HostControl, bool> stop, Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue,
            Action<T, HostControl> shutdown, Action<T, HostControl, SessionChangeReason, int> sessionevent, ServiceEvents serviceEvents)
    {
            _serviceFactory = serviceFactory;
            _start = start;
            _stop = stop;
            _pause = pause;
            _continue = @continue;
            _shutdown = shutdown;
            _sessionevent = sessionevent;
            _serviceEvents = serviceEvents;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                T service = _serviceFactory(settings);

                return new DelegateServiceHandle(service, _start, _stop, _pause, _continue, _shutdown, _sessionevent, _serviceEvents);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        class DelegateServiceHandle :
            ServiceHandle
        {
            readonly Func<T, HostControl, bool> _continue;
            readonly Func<T, HostControl, bool> _pause;
            readonly T _service;
            readonly ServiceEvents _serviceEvents;
            readonly Action<T, HostControl> _shutdown;
            readonly Func<T, HostControl, bool> _start;
            readonly Func<T, HostControl, bool> _stop;
            readonly Action<T, HostControl, SessionChangeReason, int> _sessionevent;

            public DelegateServiceHandle(T service, Func<T, HostControl, bool> start, Func<T, HostControl, bool> stop,
                Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue, Action<T, HostControl> shutdown,
                Action<T, HostControl, SessionChangeReason, int> sessionevent, ServiceEvents serviceEvents)
            {
                _service = service;
                _start = start;
                _stop = stop;
                _pause = pause;
                _continue = @continue;
                _shutdown = shutdown;
                _sessionevent = sessionevent;
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

                bool started = _start(_service, hostControl);
                if (started)
                {
                    _serviceEvents.AfterStart(hostControl);
                }
                return started;
            }

            public bool Pause(HostControl hostControl)
            {
                return _pause != null && _pause(_service, hostControl);
            }

            public bool Continue(HostControl hostControl)
            {
                return _continue != null && _continue(_service, hostControl);
            }

            public bool Stop(HostControl hostControl)
            {
                _serviceEvents.BeforeStop(hostControl);

                bool stopped = _stop(_service, hostControl);
                if (stopped)
                {
                    _serviceEvents.AfterStop(hostControl);
                }
                return stopped;
            }

            public void Shutdown(HostControl hostControl)
            {
                if(_shutdown != null)
                    _shutdown(_service, hostControl);
            }

            public void SessionEvent(HostControl hostControl, SessionChangeReason reason, int id)
            {
                if (_sessionevent != null)
                    _sessionevent(_service, hostControl, reason, id);
            }
        }
    }
}