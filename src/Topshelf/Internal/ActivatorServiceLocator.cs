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
namespace Topshelf
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Practices.ServiceLocation;

    public class ActivatorServiceLocator :
        IServiceLocator
    {
        #region IServiceLocator Members

        public object GetService(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return Activator.CreateInstance(serviceType);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return new[] {Activator.CreateInstance(serviceType)};
        }

        public TService GetInstance<TService>()
        {
            return (TService) Activator.CreateInstance(typeof(TService));
        }

        public TService GetInstance<TService>(string key)
        {
            return (TService) Activator.CreateInstance(typeof(TService));
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return new[] {(TService) Activator.CreateInstance(typeof(TService))};
        }

        #endregion
    }
}