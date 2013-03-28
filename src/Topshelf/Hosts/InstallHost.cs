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
    using System.Linq;
    using System.ServiceProcess;
    using Logging;
    using Runtime;

    public class InstallHost :
        Host
    {
        static readonly LogWriter _log = HostLogger.Get<InstallHost>();

        readonly HostEnvironment _environment;
        readonly InstallHostSettings _installSettings;
        readonly IEnumerable<Action<InstallHostSettings>> _postActions;
        readonly IEnumerable<Action<InstallHostSettings>> _preActions;
        readonly IEnumerable<Action<InstallHostSettings>> _postRollbackActions;
        readonly IEnumerable<Action<InstallHostSettings>> _preRollbackActions;
        readonly HostSettings _settings;
        readonly bool _sudo;

        public InstallHost(HostEnvironment environment, HostSettings settings, HostStartMode startMode,
            IEnumerable<string> dependencies,
            Credentials credentials, IEnumerable<Action<InstallHostSettings>> preActions,
            IEnumerable<Action<InstallHostSettings>> postActions,
            IEnumerable<Action<InstallHostSettings>> preRollbackActions,
            IEnumerable<Action<InstallHostSettings>> postRollbackActions,
            bool sudo)
        {
            _environment = environment;
            _settings = settings;

            _installSettings = new InstallServiceSettingsImpl(settings, credentials, startMode, dependencies.ToArray());

            _preActions = preActions;
            _postActions = postActions;
            _preRollbackActions = preRollbackActions;
            _postRollbackActions = postRollbackActions;
            _sudo = sudo;
        }

        public InstallHostSettings InstallSettings
        {
            get { return _installSettings; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public TopshelfExitCode Run()
        {
            if (_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is already installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceAlreadyInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo)
                {
                    if (_environment.RunAsAdministrator())
                        return TopshelfExitCode.Ok;
                }

                _log.ErrorFormat("The {0} service can only be installed as an administrator", _settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Attempting to install '{0}'", _settings.ServiceName);

            _environment.InstallService(_installSettings, ExecutePreActions, ExecutePostActions, ExecutePreRollbackActions, ExecutePostRollbackActions);

            return TopshelfExitCode.Ok;
        }

        void ExecutePreActions()
        {
            foreach (Action<InstallHostSettings> action in _preActions)
            {
                action(_installSettings);
            }
        }

        void ExecutePostActions()
        {
            foreach (Action<InstallHostSettings> action in _postActions)
            {
                action(_installSettings);
            }
        }

        void ExecutePreRollbackActions()
        {
            foreach (Action<InstallHostSettings> action in _preRollbackActions)
            {
                action(_installSettings);
            }
        }

        void ExecutePostRollbackActions()
        {
            foreach (Action<InstallHostSettings> action in _postRollbackActions)
            {
                action(_installSettings);
            }
        }

        class InstallServiceSettingsImpl :
            InstallHostSettings
        {
            readonly Credentials _credentials;
            readonly string[] _dependencies;
            readonly HostSettings _settings;
            readonly HostStartMode _startMode;

            public InstallServiceSettingsImpl(HostSettings settings, Credentials credentials, HostStartMode startMode,
                string[] dependencies)
            {
                _credentials = credentials;
                _settings = settings;
                _startMode = startMode;
                _dependencies = dependencies;
            }

            public string Name
            {
                get { return _settings.Name; }
            }

            public string DisplayName
            {
                get { return _settings.DisplayName; }
            }

            public string Description
            {
                get { return _settings.Description; }
            }

            public string InstanceName
            {
                get { return _settings.InstanceName; }
            }

            public string ServiceName
            {
                get { return _settings.ServiceName; }
            }

            public bool CanPauseAndContinue
            {
                get { return _settings.CanPauseAndContinue; }
            }

            public bool CanHandleSessionChangeEvent
            {
                get { return _settings.CanHandleSessionChangeEvent; }
            }

            public bool CanShutdown
            {
                get { return _settings.CanShutdown; }
            }

            public ServiceAccount Account
            {
                get { return _credentials.Account; }
            }

            public string Username
            {
                get { return _credentials.Username; }
            }

            public string Password
            {
                get { return _credentials.Password; }
            }

            public string[] Dependencies
            {
                get { return _dependencies; }
            }

            public HostStartMode StartMode
            {
                get { return _startMode; }
            }
        }
    }
}