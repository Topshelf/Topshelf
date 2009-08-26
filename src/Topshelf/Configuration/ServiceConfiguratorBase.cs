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
    using Microsoft.Practices.ServiceLocation;

    public class ServiceConfiguratorBase<TService> :
        IDisposable
    {
        protected Action<TService> _continueAction = NoOp;
        protected Func<IServiceLocator> _createServiceLocator = () => new ActivatorServiceLocator();
        bool _disposed;
        protected string _name = typeof (TService).Name;
        protected Action<TService> _pauseAction = NoOp;
        protected Action<TService> _startAction = NoOp;
        protected Action<TService> _stopAction = NoOp;

        public ServiceConfiguratorBase(string name)
        {
            _name = name;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void WhenStarted(Action<TService> startAction)
        {
            _startAction = startAction;
        }

        public void WhenStopped(Action<TService> stopAction)
        {
            _stopAction = stopAction;
        }

        public void WhenPaused(Action<TService> pauseAction)
        {
            _pauseAction = pauseAction;
        }

        public void WhenContinued(Action<TService> continueAction)
        {
            _continueAction = continueAction;
        }

        public void CreateServiceLocator(Func<IServiceLocator> createServiceLocator)
        {
            _createServiceLocator = createServiceLocator;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_disposed) return;

            _startAction = null;
            _stopAction = null;
            _pauseAction = null;
            _continueAction = null;

            _createServiceLocator = null;

            _disposed = true;
        }

        ~ServiceConfiguratorBase()
        {
            Dispose(false);
        }

        static void NoOp(TService instance)
        {
        }
    }
}