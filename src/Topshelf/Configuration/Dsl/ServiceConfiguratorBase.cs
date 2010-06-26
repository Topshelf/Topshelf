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
namespace Topshelf.Configuration.Dsl
{
    using System;
    using Model;

    public class ServiceConfiguratorBase<TService> :
        IDisposable
    {
        bool _disposed;
       
        public string Name { get; private set; }
        public Action<TService> ContinueAction { get; private set;}
        public Action<TService> PauseAction { get; private set;}
        public Action<TService> StartAction { get; private set; }
        public Action<TService> StopAction { get; private set; }

        public ServiceBuilder BuildAction { get; private set; }

        public ServiceConfiguratorBase()
        {
             PauseAction = NoOp;
             StartAction = NoOp;
             StopAction = NoOp;
             ContinueAction = NoOp;

             BuildAction = name =>
             {
                 var asl = new ActivatorServiceLocator();
                 return asl.GetInstance<TService>(name);
             };

            Name = "{0}/{1}".FormatWith(typeof(TService).Name,  Guid.NewGuid());
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void Named(string name)
        {
            Name = name;
        }

        public void WhenStarted(Action<TService> startAction)
        {
            StartAction = startAction;
        }

        public void WhenStopped(Action<TService> stopAction)
        {
            StopAction = stopAction;
        }

        public void WhenPaused(Action<TService> pauseAction)
        {
            PauseAction = pauseAction;
        }

        public void WhenContinued(Action<TService> continueAction)
        {
            ContinueAction = continueAction;
        }

        public void HowToBuildService(ServiceBuilder builder)
        {
            BuildAction = builder;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_disposed) return;

            StartAction = null;
            StopAction = null;
            PauseAction = null;
            ContinueAction = null;
            BuildAction = null;

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