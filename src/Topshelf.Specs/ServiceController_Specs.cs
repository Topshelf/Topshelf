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
	using System.Threading;
	using Magnum.Channels;
	using Magnum.Extensions;
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
			using (var startEvent = new ManualResetEvent(false))
			{
				_srv = new TestService();

				_channelAdaptor = new ChannelAdapter();
				_hostChannel = WellknownAddresses.GetHostHost(_channelAdaptor);

				_connection = _channelAdaptor.Connect(config => config.AddConsumerOf<ServiceStarted>().UsingConsumer(msg => startEvent.Set()));

				ServiceConfigurator<TestService> c = new ServiceConfigurator<TestService>();
				c.WhenStarted(s => s.Start());
				c.WhenStopped(s => s.Stop());
				c.WhenPaused(s => { _wasPaused = true; });
				c.WhenContinued(s => { _wasContinued = true; });
				c.HowToBuildService(name => _srv);

				_serviceController = c.Create();
				_serviceController.Start();

				startEvent.WaitOne(5.Seconds());

				_serviceController.State.ShouldEqual(ServiceState.Started);
			}
		}

		[TearDown]
		public void TearDown()
		{
			_connection.Disconnect();
			_connection.Dispose();
			_serviceController.Dispose();
			_hostChannel.Dispose();
		}

		[Test]
		public void Should_continue()
		{
			_serviceController.Pause();

			_serviceController.Continue();

			_serviceController.State
				.ShouldEqual(ServiceState.Started);
			_wasContinued
				.ShouldBeTrue();
		}

		[Test]
		public void Should_expose_contained_type()
		{
			_serviceController.ServiceType
				.ShouldEqual(typeof(TestService));
		}

		[Test]
		public void Should_pause()
		{
			_serviceController.Pause();

			_serviceController.State
				.ShouldEqual(ServiceState.Paused);

			_wasPaused
				.ShouldBeTrue();
		}

		[Test]
		public void Should_start()
		{
			_serviceController.State
				.ShouldEqual(ServiceState.Started);

			_srv.Stopped
				.ShouldBeFalse();
			_srv.Started
				.ShouldBeTrue();
		}

		[Test]
		public void Should_stop()
		{
			_serviceController.Stop();

			_serviceController.State
				.ShouldEqual(ServiceState.Stopped);

			_srv.Stopped
				.ShouldBeTrue();
		}

		IServiceController _serviceController;
		TestService _srv;
		bool _wasPaused;
		bool _wasContinued;
		ChannelAdapter _channelAdaptor;
		WcfChannelHost _hostChannel;
		ChannelConnection _connection;

		//TODO: state transition tests
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

			IServiceController service = c.Create();
			service.Start();

			service.State
				.ShouldEqual(ServiceState.Started);
		}
	}
}