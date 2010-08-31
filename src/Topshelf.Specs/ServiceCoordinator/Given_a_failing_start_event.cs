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
	using Magnum.Channels;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using Model;
	using NUnit.Framework;
	using TestObject;


	[Scenario]
	public class Given_a_failing_start_event :
		ServiceCoordinator_SpecsBase
	{
		FutureChannel<ServiceFault> _faultHappened = new FutureChannel<ServiceFault>();
		ChannelConnection _connection;

		[When]
		public void A_registered_service_throws_on_start()
		{
			_connection = Coordinator.EventChannel.Connect(x => x.AddChannel(_faultHappened));

			CreateService("test",
			              x => { throw new Exception(); },
			              x => x.Stop(),
			              x => x.Pause(),
			              x => x.Continue(),
			              (x, c) => new TestService());
		}

		[After]
		public void After()
		{
			_connection.Dispose();
			_connection = null;
		}

		[Then]
		public void An_exception_is_thrown_when_service_is_started()
		{
			Assert.That(() => Coordinator.Start(), Throws.InstanceOf<Exception>());
			_faultHappened.WaitUntilCompleted(10.Seconds()).ShouldBeTrue();
			_faultHappened.Value.ServiceName.ShouldEqual("test");
			
			IServiceController service = Coordinator.Where(x => x.Name == "test").FirstOrDefault();
			service.ShouldNotBeNull();

			service.CurrentState.ShouldEqual(ServiceStateMachine.Faulted);
		}
	}
}