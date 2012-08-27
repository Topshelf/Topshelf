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
    using System.Collections.Generic;
    using Logging;
    using Runtime;

    public class UninstallHost :
        Host
    {
        static readonly LogWriter _log = HostLogger.Get<UninstallHost>();

        readonly HostEnvironment _environment;
        readonly IEnumerable<Action> _postActions;
        readonly IEnumerable<Action> _preActions;
        readonly HostSettings _settings;
        readonly bool _sudo;


        public UninstallHost(HostEnvironment environment, HostSettings settings, IEnumerable<Action> preActions,
            IEnumerable<Action> postActions,
            bool sudo)
        {
            _environment = environment;
            _settings = settings;
            _preActions = preActions;
            _postActions = postActions;
            _sudo = sudo;
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo)
                {
                    if (_environment.RunAsAdministrator())
                        return TopshelfExitCode.Ok;
                }

                _log.ErrorFormat("The {0} service can only be uninstalled as an administrator", _settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Uninstalling {0}", _settings.ServiceName);

            _environment.UninstallService(_settings, ExecutePreActions, ExecutePostActions);

            return TopshelfExitCode.Ok;
        }

        void ExecutePreActions()
        {
            foreach (Action action in _preActions)
            {
                action();
            }
        }

        void ExecutePostActions()
        {
            foreach (Action action in _postActions)
            {
                action();
            }
        }
    }
}