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
    using Logging;
    using Runtime;

    public class StopHost :
        Host
    {
        static readonly LogWriter _log = HostLogger.Get<StopHost>();
        readonly HostEnvironment _environment;
        readonly HostSettings _settings;

        public StopHost(HostEnvironment environment, HostSettings settings)
        {
            _environment = environment;
            _settings = settings;
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                string message = string.Format("The {0} service is not installed.", _settings.ServiceName);
                _log.Error(message);

                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (!_environment.RunAsAdministrator())
                    _log.ErrorFormat("The {0} service can only be stopped by an administrator", _settings.ServiceName);

                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Stopping {0}", _settings.ServiceName);

            try
            {
                _environment.StopService(_settings.ServiceName, _settings.StopTimeOut);

                _log.InfoFormat("The {0} service was stopped.", _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _log.Error("The service failed to stop.", ex);
                return TopshelfExitCode.StopServiceFailed;
            }
        }
    }
}