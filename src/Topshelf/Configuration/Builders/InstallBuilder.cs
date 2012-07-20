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
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;
    using Hosts;
    using Runtime;

    public class InstallBuilder :
        HostBuilder
    {
        readonly IList<string> _dependencies;
        readonly HostEnvironment _environment;
        readonly IList<Action> _postActions;
        readonly IList<Action> _preActions;
        readonly HostSettings _settings;
        Credentials _credentials;
        ServiceStartMode _startMode;
        bool _sudo;

        public InstallBuilder(HostEnvironment environment, HostSettings settings)
        {
            _preActions = new List<Action>();
            _postActions = new List<Action>();
            _dependencies = new List<string>();
            _startMode = ServiceStartMode.Automatic;
            _credentials = new Credentials("", "", ServiceAccount.LocalSystem);

            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public Host Build()
        {
            return new InstallHost(_environment, _settings, _startMode, _dependencies.ToArray(), _credentials,
                _preActions,
                _postActions, _sudo);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        public void RunAs(string username, string password, ServiceAccount accountType)
        {
            _credentials = new Credentials(username, password, accountType);
        }

        public void Sudo()
        {
            _sudo = true;
        }

        public void SetStartMode(ServiceStartMode startMode)
        {
            _startMode = startMode;
        }

        public void BeforeInstall(Action callback)
        {
            _preActions.Add(callback);
        }

        public void AfterInstall(Action callback)
        {
            _postActions.Add(callback);
        }

        public void AddDependency(string name)
        {
            _dependencies.Add(name);
        }
    }
}