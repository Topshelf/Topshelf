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
    using System.Collections.Generic;
    using Magnum.Reflection;

    public class ActivatorServiceLocator
    {

        public object GetService(Type serviceType)
        {
			return FastActivator.Create(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return FastActivator.Create(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return FastActivator.Create(serviceType);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return new[] {FastActivator.Create(serviceType)};
        }

        public TService GetInstance<TService>()
        {
            return FastActivator<TService>.Create();
        }

        public TService GetInstance<TService>(string key)
        {
			return FastActivator<TService>.Create();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
			return new[] { FastActivator<TService>.Create() };
        }
    }
}