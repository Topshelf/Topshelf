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
	using System.Collections.Generic;
	using Magnum;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Model;
	using TestObject;


	[Scenario]
	public class Given_a_partially_failing_start_event :
		ServiceCoordinator_SpecsBase
	{
		Future<Exception> _future;

		[When]
		public void A_single_registered_service_throws_on_start()
		{
			_future = new Future<Exception>();
			ServiceCoordinator.ShelfFaulted += _future.Complete;
			IList<Func<IServiceController>> services = new List<Func<IServiceController>>
				{
					() => new ServiceController<TestService>("test", AddressRegistry.GetOutboundCoordinatorChannel(),
					                               x => { throw new Exception(); },
					                               x => x.Stop(),
					                               x => x.Pause(),
					                               x => x.Continue(),
					                               x => new TestService()),
					() => new ServiceController<TestService>("test2", AddressRegistry.GetOutboundCoordinatorChannel(),
					                               x => x.Start(),
					                               x => x.Stop(),
					                               x => x.Pause(),
					                               x => x.Continue(),
					                               x => new TestService())
				};

			ServiceCoordinator.RegisterServices(services);

			ServiceCoordinator.Start();
		}

		[Then]
		[Slow]
		public void The_coordinator_starts_and_invokes_the_shelf_faulted_event()
		{
			_future.WaitUntilCompleted(5.Seconds()).ShouldBeTrue();
		}
	}
}