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
    using Topshelf.Runtime;

    public class RehabServiceHandle<T> :
        ServiceHandle
        where T : class
    {
        readonly AppDomain _appDomain;
        readonly ServiceHandle _service;

        public RehabServiceHandle(AppDomain appDomain, AppDomainServiceHandle handle)
        {
            _appDomain = appDomain;
            _service = handle;
        }

        public void Dispose()
        {
            try
            {
                _service.Dispose();
            }
            finally
            {
                AppDomain.Unload(_appDomain);
            }
        }

        public bool Start(HostControl hostControl)
        {
            var control = new AppDomainHostControl(hostControl);
            return _service.Start(control);
        }

        public bool Pause(HostControl hostControl)
        {
            var control = new AppDomainHostControl(hostControl);
            return _service.Pause(control);
        }

        public bool Continue(HostControl hostControl)
        {
            var control = new AppDomainHostControl(hostControl);
            return _service.Continue(control);
        }

        public bool Stop(HostControl hostControl)
        {
            var control = new AppDomainHostControl(hostControl);
            return _service.Stop(control);
        }
    }
}