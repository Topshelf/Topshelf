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
namespace Topshelf.Model
{
    using System;

    public class IsolatedServiceControllerWrapper<TService> :
        IServiceControllerOf<object> where TService : class
    {
        readonly ServiceController<TService> _target = new ServiceController<TService>();

        #region IServiceControllerOf<object> Members

        public void Dispose()
        {
            _target.Dispose();
        }

        public Type ServiceType
        {
            get { return _target.ServiceType; }
        }

        public string Name
        {
            get { return _target.Name; }
            set { _target.Name = value; }
        }

        public ServiceState State
        {
            get { return _target.State; }
        }

        public void Start()
        {
            _target.Start();
        }

        public void Stop()
        {
            _target.Stop();
        }

        public void Pause()
        {
            _target.Pause();
        }

        public void Continue()
        {
            _target.Continue();
        }

        public Action<object> StartAction
        {
            get { return service => _target.StartAction((TService) service); }
            set { _target.StartAction = service => value(service); }
        }

        public Action<object> StopAction
        {
            get { return service => _target.StopAction((TService) service); }
            set { _target.StopAction = service => value(service); }
        }

        public Action<object> PauseAction
        {
            get { return service => _target.PauseAction((TService) service); }
            set { _target.PauseAction = service => value(service); }
        }

        public Action<object> ContinueAction
        {
            get { return service => _target.ContinueAction((TService) service); }
            set { _target.ContinueAction = service => value(service); }
        }

        public Func<ServiceBuilder> BuildServiceAction
        {
            get { return () => _target.BuildService; }
            set { _target.BuildService = value(); }
        }

        public ServiceBuilder BuildService
        {
            get { return _target.BuildService; }
            set { _target.BuildService = value; }
        }
        #endregion

        public void ChangeName(string value)
        {
            _target.Name = value;
        }
    }
}