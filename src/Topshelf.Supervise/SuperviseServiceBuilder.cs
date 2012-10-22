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
namespace Topshelf.Supervise
{
    using System;
    using Builders;
    using HostConfigurators;
    using Runtime;

    public class SuperviseServiceBuilder :
        ServiceBuilder
    {
        readonly ServiceBuilderFactory _serviceBuilderFactory;
        readonly ServiceEvents _serviceEvents;

        public SuperviseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory, ServiceEvents serviceEvents)
        {
            _serviceBuilderFactory = serviceBuilderFactory;
            _serviceEvents = serviceEvents;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                var builder = new ControlServiceBuilder<SuperviseService>(CreateSuperviseService, _serviceEvents);

                ServiceHandle serviceHandle = builder.Build(settings);

                return serviceHandle;
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating supervise service", ex);
            }
        }

        SuperviseService CreateSuperviseService(HostSettings settings)
        {

            var service = new SuperviseService(settings, _serviceBuilderFactory);
            
            ServiceAvailability serviceAvailability = new DownFileServiceAvailability(service);
            service.AddServiceAvailability(serviceAvailability);

            return service;
        }
    }
}