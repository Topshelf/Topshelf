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
	using Magnum.Extensions;
	using Model;
	using NUnit.Framework;
	using TestObject;


	[TestFixture]
	[Explicit]
	public class ServiceCoordinator_Specs
	{
		[SetUp]
		public void EstablishContext()
		{
			_service = new TestService();
			_service2 = new TestService2();

			_beforeStartingServicesInvoked = false;
			_afterStartingServicesInvoked = false;
			_afterStoppingServicesInvoked = false;

			_serviceCoordinator = new ServiceCoordinator(x => { _beforeStartingServicesInvoked = true; },
			                                             x => { _afterStartingServicesInvoked = true; },
			                                             x => { _afterStoppingServicesInvoked = true; },
			                                             10.Seconds());

			IList<Func<IService>> services = new List<Func<IService>>
				{
					() => new Service<TestService>("test", AddressRegistry.GetServiceCoordinatorProxy(),
					                               x => x.Start(),
					                               x => x.Stop(),
					                               x => x.Pause(),
					                               x => x.Continue(),
					                               x => _service),
					() => new Service<TestService>("test2", AddressRegistry.GetServiceCoordinatorProxy(),
					                               x => x.Start(),
					                               x => x.Stop(),
					                               x => x.Pause(),
					                               x => x.Continue(),
					                               x => _service2)
				};

			_serviceCoordinator.RegisterServices(services);
		}

		[TearDown]
		public void CleanUp()
		{
			_serviceCoordinator.Dispose();
		}

		[Test]
		public void Continuing_the_coordinator_should_continue_services()
		{
			_serviceCoordinator.Start();
			_serviceCoordinator.Pause();
			_serviceCoordinator.Continue();

			_service.WasRunning
				.ShouldBeTrue();
			_service.WasContinued
				.ShouldBeTrue();
			_service.Running
				.ShouldBeTrue();
		}

		[Test]
		public void Pausing_the_coordinator_should_pause_services()
		{
			_serviceCoordinator.Start();
			_serviceCoordinator.Pause();

			_service.WasRunning
				.ShouldBeTrue();
			_service.Paused
				.ShouldBeTrue();
		}

		[Test]
		public void Starting_the_coordinator_should_start_all_services()
		{
			bool beforeStartingServicesWasInvokedBeforeServiceStart = false;
			bool afterStartingServicesWasInvokedBeforeServiceStart = false;

			_service.StartAction = () =>
				{
					beforeStartingServicesWasInvokedBeforeServiceStart = _beforeStartingServicesInvoked;
					afterStartingServicesWasInvokedBeforeServiceStart = _afterStartingServicesInvoked;
				};

			_serviceCoordinator.Start();
			_service.WasRunning
				.ShouldBeTrue();
			_service2.WasRunning
				.ShouldBeTrue();

			_beforeStartingServicesInvoked
				.ShouldBeTrue();
			_afterStartingServicesInvoked
				.ShouldBeTrue();
			beforeStartingServicesWasInvokedBeforeServiceStart
				.ShouldBeTrue();
			afterStartingServicesWasInvokedBeforeServiceStart
				.ShouldBeFalse();
		}

		[Test]
		public void Stopping_the_coordinator_should_stop_all_services()
		{
			_serviceCoordinator.Start();
			_serviceCoordinator.Stop();

			_service.WasRunning
				.ShouldBeTrue();
			_service.Stopped
				.ShouldBeTrue();

			_service2.WasRunning
				.ShouldBeTrue();
			_service2.Stopped
				.ShouldBeTrue();

			_afterStoppingServicesInvoked
				.ShouldBeTrue();
		}

		[Test]
		public void Unstarted_coordinator_should_not_start_services()
		{
			_service.WasRunning
				.ShouldBeFalse();
		}

		[Test]
		[Ignore]
		public void You_cant_continue_if_you_arent_currently_paused()
		{
			Assert.Fail();
		}

		[Test]
		[Ignore]
		public void You_cant_pause_if_you_havent_started()
		{
			Assert.Fail();
		}

		TestService _service;
		TestService2 _service2;
		ServiceCoordinator _serviceCoordinator;
		bool _beforeStartingServicesInvoked;
		bool _afterStartingServicesInvoked;
		bool _afterStoppingServicesInvoked;
	}
}