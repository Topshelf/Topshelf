﻿// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Topshelf.Specs.ServiceCoordinator
{
    using System;
    using System.Collections.Generic;
    using Magnum.TestFramework;
    using Model;
    using NUnit.Framework;
    using Shelving;
    using TestObject;


    [Scenario]
    public class Given_a_failing_stop_event :
        ServiceCoordinator_SpecsBase
    {
        [When]
        public void A_registered_service_throws_on_stop()
        {
            IList<Func<IServiceController>> services = new List<Func<IServiceController>>
                {
                    () => new ServiceController<TestService>("test", WellknownAddresses.GetServiceCoordinatorProxy())
                        {
                            BuildService = s => new TestService(),
                            StartAction = x => x.Start(),
                            StopAction = x => { throw new Exception(); },
                            ContinueAction = x => x.Continue(),
                            PauseAction = x => x.Pause()
                        }
                };

            ServiceCoordinator.RegisterServices(services);

            ServiceCoordinator.Start();
        }

        [Then]
        [Slow]
        public void An_exception_is_thrown_when_service_is_stopped()
        {
            Assert.That(() => ServiceCoordinator.Stop(), Throws.InstanceOf<Exception>());
        }
    }
}