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
    using System.Threading;
    using Magnum.Extensions;
    using Magnum.TestFramework;
    using Shelving;
    using Model;
    using NUnit.Framework;
    using TestObject;


    [TestFixture, Slow]
    public class ServiceCoordinator_Singles_Specs
    {
        #region Setup/Teardown

        [SetUp]
        public void EstablishContext()
        {
            _service = new TestService();
            _service2 = new TestService2();

            _serviceCoordinator = new ServiceCoordinator(x => { }, x => { }, x => { }, 10.Seconds());
            IList<Func<IServiceController>> services = new List<Func<IServiceController>>
                                                       {
                                                           () =>
                                                           new ServiceController<TestService>("test", WellknownAddresses.GetServiceCoordinatorProxy())
                                                           {
                                                               BuildService = s=> _service,
                                                               StartAction = x => x.Start(),
                                                               StopAction = x => x.Stop(),
                                                               ContinueAction = x => x.Continue(),
                                                               PauseAction = x => x.Pause()
                                                           },
                                                           () =>
                                                           new ServiceController<TestService2>("test2", WellknownAddresses.GetServiceCoordinatorProxy())
                                                           {
                                                               BuildService = s=> _service2,
                                                               StartAction = x => x.Start(),
                                                               StopAction = x => x.Stop(),
                                                               ContinueAction = x => x.Continue(),
                                                               PauseAction = x => x.Pause()
                                                           }
                                                       };

            _serviceCoordinator.RegisterServices(services);
            _serviceCoordinator.Start();

        	Thread.Sleep(2.Seconds());
        }

		[TearDown]
		public void CleanUp()
		{
			_serviceCoordinator.Dispose();
		}

        #endregion

        TestService _service;
        TestService2 _service2;
        ServiceCoordinator _serviceCoordinator;

        [Test]
        public void D_Continue_individual_service()
        {
            _serviceCoordinator.PauseService("test");
            _serviceCoordinator.ContinueService("test");

            _service.Started
                .ShouldBeTrue();
            _service.HasBeenContinued
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }

        [Test]
        public void Pause_individual_service()
        {
            _serviceCoordinator.PauseService("test");

            _service.Paused
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }

        [Test]
        public void Start_individual_service()
        {
            _serviceCoordinator.Pause(); //should pause them all?
            _serviceCoordinator.ContinueService("test");

            _service.Started
                .ShouldBeTrue();
            _service2.Paused
                .ShouldBeTrue();
        }

        [Test]
        public void Stop_individual_service()
        {
            _serviceCoordinator.StopService("test");

            _service.Stopped
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }
    }
}