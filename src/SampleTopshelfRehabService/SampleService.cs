// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace SampleTopshelfRehabService
{
    using System;
    using System.Threading;
    using Topshelf;
    using Topshelf.Logging;


    class SampleService :
        ServiceControl,
        ServiceSuspend,
        ServiceShutdown,
        ServiceSessionChange,
        ServiceCustomCommand
    {
        LogWriter _log;

        public bool Start(HostControl hostControl)
        {
            _log = HostLogger.Get<SampleService>();

            _log.Info("SampleService Starting...");

            hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));

            Thread.Sleep(1000);

            ThreadPool.QueueUserWorkItem(x =>
                {
                    Thread.Sleep(3000);

                    _log.Info("Requesting a restart!!!");

                    hostControl.Restart();

//                _log.Info("Dying an ungraceful death");
//
//                throw new InvalidOperationException("Oh, what a world.");
                });
            _log.Info("SampleService Started");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("SampleService Stopped");

            return true;
        }

        public void SessionChange(HostControl hostControl, SessionChangedArguments changedArguments)
        {
            _log.Info("Service session changed");
        }

        public void Shutdown(HostControl hostControl)
        {
            _log.Info("Service is being shutdown, bye!");
        }

        public bool Pause(HostControl hostControl)
        {
            _log.Info("SampleService Paused");

            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            _log.Info("SampleService Continued");

            return true;
        }

        public void CustomCommand(HostControl hostControl, int command)
        {
            _log.Info(string.Format("Custom command received: {0}", command));
        }
    }
}