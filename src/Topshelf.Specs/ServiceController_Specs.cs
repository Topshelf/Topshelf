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
namespace Topshelf.Specs
{
	using System;
	using Magnum.Channels;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using Model;
	using NUnit.Framework;
	using TestObject;
	using Topshelf.Configuration.Dsl;


	[TestFixture]
	public class ServiceController_Specs
	{
		[TearDown]
		public void TearDown()
		{
			_serviceController.Dispose();
			_hostChannel.Dispose();
		}

		[SetUp]
		public void EstablishContext()
		{
			_serviceStarted = new FutureChannel<ServiceRunning>();
			_service = new TestService();
			_hostChannel = new TestChannel();


			using (var c = new ServiceConfigurator<TestService>())
			{
				c.Named("test");
				c.WhenStarted(s => s.Start());
				c.WhenStopped(s => s.Stop());
				c.WhenPaused(s => { });
				c.WhenContinued(s => { });
				c.HowToBuildService(name => _service);

				_serviceController = c.Create(null, _hostChannel);
			}

			_hostChannel.Connect(x =>
				{
					x.AddConsumersFor<ServiceStateMachine>()
						.BindUsing<ServiceStateMachineBinding, string>()
						.CreateNewInstanceBy(GetServiceInstance)
						.HandleOnInstanceFiber()
						.PersistInMemory();

					x.AddChannel(_serviceStarted);
				});

			_hostChannel.Send(new CreateService("test"));
			//_serviceChannel.Send(new StartService("test"));

			_serviceStarted.WaitUntilCompleted(5.Seconds());
		}

		[Test]
		[Slow]
		[Explicit("Not Yet Implemented")]
		public void Should_continue()
		{
			_hostChannel.Send(new PauseService("test"));
			_hostChannel.Send(new ContinueService("test"));

			_serviceController.ShouldBeRunning();
			_service.WasContinued.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		[Explicit("Typing error")]
		public void Should_expose_contained_type()
		{
			_serviceController.ServiceType
				.ShouldEqual(typeof(TestService));
		}

		[Test]
		[Slow]
		[Explicit("Not Yet Implemented")]
		public void Should_pause()
		{
			_hostChannel.Send(new PauseService("test"));

			_serviceController.ShouldBePaused();
			_service.Paused.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		public void Should_start()
		{
			_serviceController.ShouldBeRunning();
			_service.Running.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		public void Should_stop()
		{
			var stopped = new FutureChannel<ServiceStopped>();
			_hostChannel.Connect(x => x.AddChannel(stopped));

			_hostChannel.Send(new StopService("test"));

			stopped.WaitUntilCompleted(5.Seconds()).ShouldBeTrue();

			_serviceController.ShouldBeStopped();
			_service.Stopped.IsCompleted.ShouldBeTrue();
		}

		ServiceStateMachine GetServiceInstance(string key)
		{
			// this is needed for refrection
			if (key == null)
				return new ServiceStateMachine(null, _hostChannel);

			if (key == _serviceController.Name)
				return _serviceController;

			throw new InvalidOperationException("An unknown service was requested: " + key);
		}

		FutureChannel<ServiceRunning> _serviceStarted;

		ServiceController<TestService> _serviceController;
		TestService _service;
		TestChannel _hostChannel;
	}


	public static class ServiceAssertions
	{
		public static void ShouldBeRunning<TService>(this ServiceController<TService> service)
			where TService : class
		{
			service.CurrentState.ShouldEqual(ServiceStateMachine.Running);
		}

		public static void ShouldBeStopped<TService>(this ServiceController<TService> service)
			where TService : class
		{
			service.CurrentState.ShouldEqual(ServiceStateMachine.Stopped);
		}

		public static void ShouldBePaused<TService>(this ServiceController<TService> service)
			where TService : class
		{
			service.CurrentState.ShouldEqual(ServiceStateMachine.Paused);
		}
	}


	[TestFixture]
	public class SimpleServiceContainerStuff
	{
		[Test]
		public void Should_work()
		{
			var c = new ServiceConfigurator<TestService>();
			c.WhenStarted(s => s.Start());
			c.WhenStopped(s => s.Stop());

			using (IServiceController service = c.Create(null, AddressRegistry.GetOutboundCoordinatorChannel()))
			{
//				service.Send(new StartService());

				//			service.State
				//			.ShouldEqual(ServiceState.Started);
			}
		}
	}
}