// Copyright 2007-2008 The Apache Software Foundation.
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
    using Model;
    using NUnit.Framework;
    using Rhino.Mocks;
    using TestObject;
    using Topshelf.Configuration.Dsl;

    [TestFixture]
    public class ServiceController_Specs
    {
        private IServiceController _serviceController;
        private TestService _srv;
        private bool _wasPaused;
        private bool _wasContinued;

        [SetUp]
        public void EstablishContext()
        {
            _srv = new TestService();

            ServiceConfigurator<TestService> c = new ServiceConfigurator<TestService>("my_service");
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());
            c.WhenPaused(s => { _wasPaused = true; });
            c.WhenContinued(s => { _wasContinued = true; });
            c.HowToBuildService((name)=> new TestService());
            _serviceController = c.Create();
            _serviceController.Start();
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

        [Test]
        public void Should_start()
        {
            //_service.Start();

            _serviceController.State
                .ShouldEqual(ServiceState.Started);

            _srv.Stopped
                .ShouldBeFalse();
            _srv.Started
                .ShouldBeTrue();
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

        //TODO: state transition tests
    }

    [TestFixture]
    public class SimpleServiceContainerStuff
    {
        [Test]
        public void Should_work()
        {
            var c = new ServiceConfigurator<TestService>("my_service");
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());

            var service = c.Create();
            service.Start();

            service.State
                .ShouldEqual(ServiceState.Started);
        }
    }
}