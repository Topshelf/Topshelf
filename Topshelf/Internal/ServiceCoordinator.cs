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
    using System.Collections.Generic;
    using Configuration;

    public class ServiceCoordinator :
        IServiceCoordinator
    {
        private readonly IDictionary<string, IService> _services = new Dictionary<string, IService>();
        private readonly Action<IServiceCoordinator> _beforeStart;

        public ServiceCoordinator(Action<IServiceCoordinator> beforeStart)
        {
            _beforeStart = beforeStart;
        }

        public void Start()
        {
            _beforeStart(this);
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
            OnStopped();
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

        public IService GetService(string name)
        {
            return _services[name];
        }

        #region Dispose Crap

        private bool _disposed;
        ~ServiceCoordinator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Stop();
            }
            _disposed = true;
        }
        #endregion

        public event Action Stopped;
        private void OnStopped()
        {
            Action handler = Stopped;
            if (handler != null)
            {
                handler();
            }
        }
    }
}