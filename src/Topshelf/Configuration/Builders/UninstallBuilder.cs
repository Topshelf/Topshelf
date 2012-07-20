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
    using Hosts;
    using Runtime;

    public class UninstallBuilder :
        HostBuilder
    {
        readonly HostEnvironment _environment;
        readonly IList<Action> _postActions;
        readonly IList<Action> _preActions;
        readonly HostSettings _settings;
        bool _sudo;

        public UninstallBuilder(HostEnvironment environment, HostSettings settings)
        {
            _preActions = new List<Action>();
            _postActions = new List<Action>();

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
            return new UninstallHost(_environment, _settings, _preActions, _postActions, _sudo);
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

        public void Sudo()
        {
            _sudo = true;
        }

        public void BeforeUninstall(Action callback)
        {
            _preActions.Add(callback);
        }

        public void AfterUninstall(Action callback)
        {
            _postActions.Add(callback);
        }
    }
}