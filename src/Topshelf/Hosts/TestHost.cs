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
namespace Topshelf.Hosts
{
    using System;
    using System.Threading;
    using Logging;
    using Runtime;

    public class TestHost :
        Host,
        HostControl
    {
        readonly HostEnvironment _environment;
        readonly LogWriter _log = HostLogger.Get<TestHost>();
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;

        public TestHost(HostSettings settings, HostEnvironment environment, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (environment == null)
                throw new ArgumentNullException("environment");

            _settings = settings;
            _environment = environment;
            _serviceHandle = serviceHandle;
        }

        public TopshelfExitCode Run()
        {
            var exitCode = TopshelfExitCode.AbnormalExit;
            try
            {
                exitCode = TopshelfExitCode.StartServiceFailed;

                _log.InfoFormat("The {0} service is being started.", _settings.ServiceName);
                _serviceHandle.Start(this);
                _log.InfoFormat("The {0} service was started.", _settings.ServiceName);

                Thread.Sleep(100);

                exitCode = TopshelfExitCode.StopServiceFailed;

                _log.InfoFormat("The {0} service is being stopped.", _settings.ServiceName);
                _serviceHandle.Stop(this);
                _log.InfoFormat("The {0} service was stopped.", _settings.ServiceName);

                exitCode = TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("The service threw an exception during testing.", ex);
            }
            finally
            {
                _serviceHandle.Dispose();
            }

            return exitCode;
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // good for you, maybe we'll use a timer for startup at some point but for debugging
            // it's a pain in the ass
        }

        void HostControl.Stop()
        {
            _log.Info("Service Stop requested, exiting.");
        }

        void HostControl.Restart()
        {
            _log.Info("Service Restart requested, but we don't support that here, so we are exiting.");
        }
    }
}