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

    public class ServiceEventsImpl :
        ServiceEvents
    {
        readonly EventCallbackList<HostStartedContext> _afterStart;
        readonly EventCallbackList<HostStoppedContext> _afterStop;
        readonly EventCallbackList<HostStartContext> _beforeStart;
        readonly EventCallbackList<HostStopContext> _beforeStop;

        public ServiceEventsImpl()
        {
            _afterStart = new EventCallbackList<HostStartedContext>();
            _afterStop = new EventCallbackList<HostStoppedContext>();
            _beforeStart = new EventCallbackList<HostStartContext>();
            _beforeStop = new EventCallbackList<HostStopContext>();
        }

        public void BeforeStart(HostControl hostControl)
        {
            var context = new HostStartContextImpl(hostControl);

            _beforeStart.Notify(context);
        }

        public void AfterStart(HostControl hostControl)
        {
            var context = new HostStartedContextImpl(hostControl);

            _afterStart.Notify(context);
        }

        public void BeforeStop(HostControl hostControl)
        {
            var context = new HostStopContextImpl(hostControl);

            _beforeStop.Notify(context);
        }

        public void AfterStop(HostControl hostControl)
        {
            var context = new HostStoppedContextImpl(hostControl);

            _afterStop.Notify(context);
        }

        public void AddBeforeStart(Action<HostStartContext> callback)
        {
            _beforeStart.Add(callback);
        }

        public void AddAfterStart(Action<HostStartedContext> callback)
        {
            _afterStart.Add(callback);
        }

        public void AddBeforeStop(Action<HostStopContext> callback)
        {
            _beforeStop.Add(callback);
        }

        public void AddAfterStop(Action<HostStoppedContext> callback)
        {
            _afterStop.Add(callback);
        }

        abstract class ContextImpl
        {
            readonly HostControl _hostControl;

            public ContextImpl(HostControl hostControl)
            {
                _hostControl = hostControl;
            }

            public void RequestAdditionalTime(TimeSpan timeRemaining)
            {
                _hostControl.RequestAdditionalTime(timeRemaining);
            }

            public void Stop()
            {
                _hostControl.Stop();
            }

            public void Stop(TopshelfExitCode exitCode)
            {
                _hostControl.Stop(exitCode);
            }
        }

        class HostStartContextImpl :
            ContextImpl,
            HostStartContext
        {
            public HostStartContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }

            public void CancelStart()
            {
                throw new ServiceControlException("The start service operation was canceled.");
            }
        }

        class HostStartedContextImpl :
            ContextImpl,
            HostStartedContext
        {
            public HostStartedContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }
        }

        class HostStopContextImpl :
            ContextImpl,
            HostStopContext
        {
            public HostStopContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }
        }

        class HostStoppedContextImpl :
            ContextImpl,
            HostStoppedContext
        {
            public HostStoppedContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }
        }
    }
}