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
namespace Topshelf.Internal
{
    using System;
    using Configuration;
    using Microsoft.Practices.ServiceLocation;

    public class Service<TService> :
        IService
    {
        public Service()
        {
            State = ServiceState.Stopped;
        }

        public Type ServiceType
        {
            get
            {
                return typeof(TService);
            }
        }

        public ServiceState State { get; private set; }
        public string Name { get; set; }
        public Action<TService> StartAction{ get; set;}
        public Action<TService> StopAction{ get; set;}
        public Action<TService> PauseAction{ get; set;}
        public Action<TService> ContinueAction{ get; set;}

        public void Start()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            StartAction(instance);
            State = ServiceState.Started;
        }

        public void Stop()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            StopAction(instance);
            State = ServiceState.Stopped;
        }

        public void Pause()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            PauseAction(instance);
            State = ServiceState.Paused;
        }

        public void Continue()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            ContinueAction(instance);
            State = ServiceState.Started;
        }
    }
}