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
namespace Topshelf.Rehab
{
    using System;
    using Topshelf.Builders;
    using Topshelf.HostConfigurators;
    using Topshelf.Runtime;

    public class AppDomainServiceHandle :
        MarshalByRefObject,
        ServiceHandle
    {
        ServiceHandle _serviceHandle;

        public bool Start(HostControl hostControl)
        {
            return _serviceHandle.Start(hostControl);
        }

        public bool Stop(HostControl hostControl)
        {
            return _serviceHandle.Stop(hostControl);
        }

        public bool Pause(HostControl hostControl)
        {
            return _serviceHandle.Pause(hostControl);
        }

        public bool Continue(HostControl hostControl)
        {
            return _serviceHandle.Continue(hostControl);
        }

        public void Dispose()
        {
            _serviceHandle.Dispose();
        }

        public void Create(ServiceBuilderFactory serviceBuilderFactory, HostSettings settings)
        {
            ServiceBuilder serviceBuilder = serviceBuilderFactory(settings);

            _serviceHandle = serviceBuilder.Build(settings);
        }
    }
}