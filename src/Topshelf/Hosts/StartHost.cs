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

    public class StartHost :
        Host
    {
        readonly HostEnvironment _environment;
        readonly LogWriter _log = HostLogger.Get<StartHost>();
        readonly Host _parentHost;
        HostSettings _settings;

        public StartHost(HostEnvironment environment, HostSettings settings, Host parentHost)
        {
            _environment = environment;
            _settings = settings;
            _parentHost = parentHost;
        }

        public StartHost(HostEnvironment environment, HostSettings settings)
            : this(environment, settings, null)
        {
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsAdministrator)
            {
                if (!_environment.RunAsAdministrator())
                    _log.ErrorFormat("The {0} service can only be started by an administrator", _settings.ServiceName);

                return TopshelfExitCode.SudoRequired;
            }

            if (_parentHost != null)
                _parentHost.Run();

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            _log.DebugFormat("Starting {0}", _settings.ServiceName);

            try
            {
                _environment.StartService(_settings.ServiceName, _settings.StartTimeOut);

                _log.InfoFormat("The {0} service was started.", _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _log.Error("The service failed to start.", ex);
                return TopshelfExitCode.StartServiceFailed;
            }
        }
    }
}