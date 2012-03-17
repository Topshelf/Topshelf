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
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using NUnit.Framework;
	using TestObject;


    [Scenario]
    public class when_the_coord_stops_all_should_stop :
        InterationContext
    {
        [Given]
        public void two_running_services()
        {
            _service = new TestService();
            _service2 = new TestService2();

            CreateService("test", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service);
            CreateService("test2", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service2);

            Coordinator.Start();

            _service.Running.WaitUntilCompleted(10.Seconds());
            _service2.Running.WaitUntilCompleted(10.Seconds());
        }

        [When]
        public void you_stop_all_services()
        {
            Coordinator.Stop();
        }

        [Then]
        public void All_services_should_stop()
        {

            _service.Stopped.WaitUntilCompleted(1.Seconds());
            _service2.Stopped.WaitUntilCompleted(1.Seconds());
        }

        TestService _service;
        TestService2 _service2;
    }

    [Scenario]
    public class services_should_be_stoppable :
        InterationContext
    {
        [Given]
        public void two_running_services()
        {
            _service = new TestService();
            _service2 = new TestService2();

            CreateService("test", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service);
            CreateService("test2", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service2);

            Coordinator.Start();

            _service.Running.WaitUntilCompleted(10.Seconds());
            _service2.Running.WaitUntilCompleted(10.Seconds());
        }

        [When]
        public void you_stop_all_services()
        {
            Coordinator.StopService("test");
            _service.Stopped.WaitUntilCompleted(1.Seconds());
        }

        [Then]
        public void the_service_should_stop()
        {
            _service.Stopped.Value.ShouldBeTrue();
        }

        [Then]
        public void service2_should_still_be_running()
        {
            _service2.Stopped.Value.ShouldBeFalse();
        }

        TestService _service;
        TestService2 _service2;
    }


    [Scenario]
    public class services_should_be_pausable_continue :
        InterationContext
    {
        [Given]
        public void two_running_services()
        {
            _service = new TestService();
            _service2 = new TestService2();

            CreateService("test", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service);
            CreateService("test2", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service2);

            Coordinator.Start();

            _service.Running.WaitUntilCompleted(10.Seconds());
            _service2.Running.WaitUntilCompleted(10.Seconds());
        }

        [When]
        public void you_pause_a_service()
        {
            Coordinator.PauseService("test");
            _service.Paused.WaitUntilCompleted(1.Seconds());
        }

        [Then]
        public void it_should_pause()
        {
            _service.Paused.IsCompleted.ShouldBeTrue();
        }

        [Then]
        public void the_other_service_should_still_be_running()
        {
            _service2.Running.IsCompleted.ShouldBeTrue();
        }

        TestService _service;
        TestService2 _service2;
    }

    [Scenario]
    public class services_should_be_continue :
        InterationContext
    {
        [Given]
        public void a_paused_service()
        {
            _service = new TestService();

            CreateService("test", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service);

            Coordinator.Start();

            _service.Running.WaitUntilCompleted(1.Seconds());

            Coordinator.PauseService("test");
            _service.Paused.WaitUntilCompleted(1.Seconds());
        }

        [When]
        public void you_continue_a_service()
        {
            Coordinator.ContinueService("test");
            _service.WasContinued.WaitUntilCompleted(1.Seconds());
        }

        [Then]
        public void it_should_continue()
        {
            _service.WasContinued.IsCompleted.ShouldBeTrue();
        }

        TestService _service;
    }

    [Scenario]
	public class services_should_start :
        InterationContext
	{
		[Given]
		public void two_services()
		{
			_service = new TestService();
			_service2 = new TestService2();

		    CreateService("test", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service);
		    CreateService("test2", x => x.Start(), x => x.Stop(), x => x.Pause(), x => x.Continue(), (x, c) => _service2);

		}

        [When]
        public void you_start_the_coordinator()
        {
			Coordinator.Start();
        }

        [Then]
        public void All_services_should_start()
        {
			_service.Running.WaitUntilCompleted(10.Seconds());
			_service2.Running.WaitUntilCompleted(10.Seconds());

            _service.Running.Value.ShouldBeTrue();
            _service2.Running.Value.ShouldBeTrue();
        }

		TestService _service;
		TestService2 _service2;
	}
}