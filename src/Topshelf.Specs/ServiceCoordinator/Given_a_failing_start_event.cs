// Copyright 2007-2010 The Apache Software Foundation.
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
	using System.Linq;
	using Magnum;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Model;
	using NUnit.Framework;
	using TestObject;


    [Scenario]
	public class Given_a_failing_start_event :
		ServiceCoordinator_SpecsBase
	{
        Future<string> _faultHappened = new Future<string>();

		[When]
		public void A_registered_service_throws_on_start()
		{
		    Coordinator.ServiceFault += (msg, ex) => _faultHappened.Complete(msg);

            CreateService<TestService>("test",
                                       x => { throw new Exception(); },
                                       x => x.Stop(),
                                       x => x.Pause(),
                                       x => x.Continue(),
                                       (x, c) => new TestService());
		}

		[Then]
		public void An_exception_is_thrown_when_service_is_started()
		{
            Assert.That(() => Coordinator.Start(), Throws.InstanceOf<Exception>());
		    _faultHappened.WaitUntilCompleted(10.Seconds()).ShouldBeTrue();
		    Coordinator.Where(x => x.Name == "test").FirstOrDefault().CurrentState.ShouldEqual(ServiceStateMachine.Faulted);
		}
	}
}