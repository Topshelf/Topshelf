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
	using Magnum.Channels;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using Model;
	using NUnit.Framework;
	using Shelving;
	using TestObject;
	using Topshelf.Configuration.Dsl;


	[TestFixture]
	public class ServiceController_Specs
	{
		[SetUp]
		public void EstablishContext()
		{
			_serviceStarted = new FutureChannel<ServiceRunning>();
			_service = new TestService();
			_hostChannel = AddressRegistry.GetServiceCoordinatorHost(x => { x.AddChannel(_serviceStarted); });

			using (var c = new ServiceConfigurator<TestService>())
			{
				c.WhenStarted(s => s.Start());
				c.WhenStopped(s => s.Stop());
				c.WhenPaused(s => { _wasPaused = true; });
				c.WhenContinued(s => { _wasContinued = true; });
				c.HowToBuildService(name => _service);

				_serviceController = c.Create(null, AddressRegistry.GetOutboundCoordinatorChannel());
			}

			_serviceChannel = new InboundChannel(AddressRegistry.GetServiceAddress(_serviceController.Name),
			                                     AddressRegistry.GetServicePipeName(_serviceController.Name), x =>
			                                     	{
			                                     		x.AddConsumersFor<ServiceStateMachine>()
			                                     			.UsingInstance(_serviceController)
															.HandleOnCallingThread();
			                                     	});

			_serviceChannel.Send(new StartService("test"));

			_serviceStarted.WaitUntilCompleted(10.Seconds());

			_serviceController.ShouldBeRunning();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceController.Dispose();
			_serviceChannel.Dispose();
			_hostChannel.Dispose();
		}

		[Test]
		[Slow]
		public void Should_continue()
		{
			_serviceChannel.Send(new PauseService("test"));
			_serviceChannel.Send(new ContinueService("test"));

			_serviceController.ShouldBeRunning();
			_service.WasContinued.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		public void Should_expose_contained_type()
		{
			_serviceController.ServiceType
				.ShouldEqual(typeof(TestService));
		}

		[Test]
		[Slow]
		public void Should_pause()
		{
			_serviceChannel.Send(new PauseService("test"));

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
			_serviceChannel.Send(new StopService("test"));

			_serviceController.ShouldBeStopped();
			_service.Stopped.IsCompleted.ShouldBeTrue();
		}

		FutureChannel<ServiceRunning> _serviceStarted;

		ServiceController<TestService> _serviceController;
		TestService _service;
		bool _wasPaused;
		bool _wasContinued;
		InboundChannel _hostChannel;
		InboundChannel _serviceChannel;

		//TODO: state transition tests
	}


	public static class ServiceAssertions
	{
		public static void ShouldBeRunning<TService>(this ServiceController<TService> service) 
			where TService : class
		{
			service.CurrentState.ShouldEqual(ServiceController<TService>.Running);
		}
	
		public static void ShouldBeStopped<TService>(this ServiceController<TService> service) 
			where TService : class
		{
			service.CurrentState.ShouldEqual(ServiceController<TService>.Stopped);
		}

		public static void ShouldBePaused<TService>(this ServiceController<TService> service) 
			where TService : class
		{
			service.CurrentState.ShouldEqual(ServiceController<TService>.Paused);
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