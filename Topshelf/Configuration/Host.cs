// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf.Configuration
{
    using System;
    using System.Collections.Generic;
    using Actions;

    public class Host :
        IHost
    {
        private readonly IDictionary<string, IService> _services = new Dictionary<string, IService>();
        private Type _formType;
        private NamedAction _action;
        public WinServiceSettings WinServiceSettings { get; set; }
        public Credentials Credentials { get; set; }

        public void Start()
        {
            foreach (var service in _services.Values)
            {
                service.Start();
            }
        }

        public void Stop()
        {
            foreach (var service in _services.Values)
            {
                service.Stop();
            }
        }

        public void Pause()
        {
            foreach (var service in _services.Values)
            {
                service.Pause();
            }
        }

        public void Continue()
        {
            foreach (var service in _services.Values)
            {
                service.Continue();
            }
        }

        public void StartService(string name)
        {
            _services[name].Start();
        }

        public void StopService(string name)
        {
            _services[name].Stop();
        }

        public void PauseService(string name)
        {
            _services[name].Pause();
        }

        public void ContinueService(string name)
        {
            _services[name].Continue();
        }

        public void RegisterServices(IList<IService> services)
        {
            foreach (var service in services)
            {
                _services.Add(service.Name, service);
            }
        }

        public int HostedServiceCount
        {
            get
            {
                return _services.Count;
            }
        }

        public void SetRunnerAction(NamedAction action, Type form)
        {
            _action = action;
            _formType = form;
        }

        public IService GetService(string name)
        {
            return _services[name];
        }
    }
}