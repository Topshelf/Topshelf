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
        static readonly Log _log = Logger.Get<InstallHost>();

        readonly HostEnvironment _environment;
        readonly InstallServiceSettings _installSettings;
        readonly IEnumerable<Action> _postActions;
        readonly IEnumerable<Action> _preActions;
        readonly HostSettings _settings;
        readonly bool _sudo;

        public InstallHost(HostEnvironment environment, HostSettings settings, ServiceStartMode startMode,
            IEnumerable<string> dependencies,
            Credentials credentials, IEnumerable<Action> preActions, IEnumerable<Action> postActions, bool sudo)
        {
            _environment = environment;
            _settings = settings;

            _installSettings = new InstallServiceSettingsImpl(settings, credentials, startMode, dependencies.ToArray());

            _preActions = preActions;
            _postActions = postActions;
            _sudo = sudo;
        }

        public void Run()
        {
            if (_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is already installed.", _settings.ServiceName);
                return;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo)
                {
                    if (_environment.RunAsAdministrator())
                        return;
                }

                _log.ErrorFormat("The {0} service can only be installed as an administrator", _settings.ServiceName);
                return;
            }

            _log.DebugFormat("Attempting to install '{0}'", _settings.ServiceName);

            _environment.InstallService(_installSettings, ExecutePreActions, ExecutePostActions);
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

        class InstallServiceSettingsImpl :
            InstallServiceSettings
        {
            Credentials _credentials;
            string[] _dependencies;
            HostSettings _settings;
            ServiceStartMode _startMode;

            public InstallServiceSettingsImpl(HostSettings settings, Credentials credentials, ServiceStartMode startMode,
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

            public string Password
            {
                get { return _credentials.Password; }
            }

            public string[] Dependencies
            {
                get { return _dependencies; }
            }

            public ServiceStartMode StartMode
            {
                get { return _startMode; }
            }

            public string Username
            {
                get { return _credentials.Username; }
            }

            public string ServiceName
            {
                get { return _settings.ServiceName; }
            }

            public ServiceAccount Account
            {
                get { return _credentials.Account; }
            }
        }
    }
}