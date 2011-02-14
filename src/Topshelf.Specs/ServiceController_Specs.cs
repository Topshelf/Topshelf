// Copyright 2007-2011 The Apache Software Foundation.
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
	using Builders;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using Model;
	using NUnit.Framework;
	using Stact;
	using TestObject;


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

			_coordinator = new Model.ServiceCoordinator();

			var b = new LocalServiceBuilder<TestService>(null, _serviceName, (d, name, coordinator) => _service,
			                                             x => x.Start(), x => x.Stop(), x => { }, x => { });


			_controllerFactory = new ServiceControllerFactory();
			_factory = _controllerFactory.CreateFactory(inbox => b.Build(inbox, _coordinator));
			_instance = _factory.GetActor();

			_hostChannel.Connect(x =>
				{
					x.AddChannel(_instance);

					x.AddChannel(_serviceStarted);
				});

			_coordinator.Send(new CreateService(_serviceName));
			_coordinator.Send(new StartService(_serviceName));

			_serviceStarted.WaitUntilCompleted(5.Seconds());
		}

		[Test]
		[Slow]
		[Explicit("Not Yet Implemented")]
		public void Should_continue()
		{
			_hostChannel.Send(new PauseService(_serviceName));
			_hostChannel.Send(new ContinueService(_serviceName));

			_serviceController.CurrentState.ShouldEqual(_controllerFactory.Workflow.GetState(x => x.Running));
			_service.WasContinued.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		[Explicit("Typing error")]
		public void Should_expose_contained_type()
		{
			_serviceController.ServiceType.ShouldEqual(typeof(TestService));
		}

		[Test]
		[Slow]
		[Explicit("Not Yet Implemented")]
		public void Should_pause()
		{
			_hostChannel.Send(new PauseService(_serviceName));

			_serviceController.CurrentState.ShouldEqual(_controllerFactory.Workflow.GetState(x => x.Paused));
			_service.Paused.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		public void Should_start()
		{
			_serviceController.CurrentState.ShouldEqual(_controllerFactory.Workflow.GetState(x => x.Running));
			_service.Running.IsCompleted.ShouldBeTrue();
		}

		[Test]
		[Slow]
		public void Should_stop()
		{
			var stopped = new FutureChannel<ServiceStopped>();
			_hostChannel.Connect(x => x.AddChannel(stopped));

			_hostChannel.Send(new StopService(_serviceName));

			stopped.WaitUntilCompleted(5.Seconds()).ShouldBeTrue();

			_serviceController.CurrentState.ShouldEqual(_controllerFactory.Workflow.GetState(x => x.Stopped));
			_service.Stopped.IsCompleted.ShouldBeTrue();
		}

		FutureChannel<ServiceRunning> _serviceStarted;

		IServiceController<TestService> _serviceController;
		TestService _service;
		TestChannel _hostChannel;
		ServiceControllerFactory _controllerFactory;
		ActorFactory<IServiceController> _factory;
		ActorInstance _instance;
		IServiceCoordinator _coordinator;
		string _serviceName = "test";
	}


	public class TestBuilder :
		HostBuilder
	{
		public ServiceDescription Description
		{
			get { throw new NotImplementedException(); }
		}

		public Host Build()
		{
			throw new NotImplementedException();
		}

		public void Match<T>(Action<T> callback)
			where T : class, HostBuilder
		{
		}
	}
}