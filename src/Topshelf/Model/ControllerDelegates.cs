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
    using Microsoft.Practices.ServiceLocation;

    [Serializable]
    public class ControllerDelegates<TService> where TService : class
    {
        public Action<TService> StartAction { get; set; }
        public Action<TService> StopAction { get; set; }
        public Action<TService> PauseAction { get; set; }
        public Action<TService> ContinueAction { get; set; }
        public Func<IServiceLocator> CreateServiceLocator { get; set; }

        public void StartActionObject(object obj)
        {
            StartAction((TService) obj);
        }

        public void StopActionObject(object obj)
        {
            StopAction((TService) obj);
        }

        public void PauseActionObject(object obj)
        {
            PauseAction((TService) obj);
        }

        public void ContinueActionObject(object obj)
        {
            ContinueAction((TService) obj);
        }
    }
}