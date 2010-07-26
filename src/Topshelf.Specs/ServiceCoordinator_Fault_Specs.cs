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
    using System.Collections.Generic;
    using Magnum;
    using Magnum.Extensions;
    using Model;
    using NUnit.Framework;
    using TestObject;


    [TestFixture]
    public class ServiceCoordinator_Fault_Specs
    {
        ServiceCoordinator _serviceCoordinator;

        [SetUp]
        public void Setup()
        {
            _serviceCoordinator = new ServiceCoordinator(x => { }, x => { }, x => { });
        }

        [TearDown]
        public void TearDown()
        {
            _serviceCoordinator.Dispose();
        }

        [Test]
        public void Fault_when_service_continues()
        {
        }

        [Test]
        public void Fault_when_service_pauses()
        {
        }

        [Test]
        public void Fault_when_service_starts()
        {
            IList<Func<IServiceController>> services = new List<Func<IServiceController>>
                {
                    () => new ServiceController<TestService>("test")
                        {
                            BuildService = s => new TestService(),
                            StartAction = x => { throw new Exception(); },
                            StopAction = x => x.Stop(),
                            ContinueAction = x => x.Continue(),
                            PauseAction = x => x.Pause()
                        }
                };

            _serviceCoordinator.RegisterServices(services);
            Assert.That(() => _serviceCoordinator.Start(), Throws.InstanceOf<Exception>());
        }

        [Test]
        public void Fault_when_service_stops()
        {
        }

        [Test, Explicit]
        public void No_exception_when_only_some_services_start()
        {
            Future<KeyValuePair<string, Exception>> future = new Future<KeyValuePair<string, Exception>>();
            _serviceCoordinator.ShelfFaulted += future.Complete;
            IList<Func<IServiceController>> services = new List<Func<IServiceController>>
                {
                    () => new ServiceController<TestService>("test")
                        {
                            BuildService = s => new TestService(),
                            StartAction = x => { throw new Exception(); },
                            StopAction = x => x.Stop(),
                            ContinueAction = x => x.Continue(),
                            PauseAction = x => x.Pause()
                        },
                    () => new ServiceController<TestService>("test2")
                        {
                            BuildService = s => new TestService(),
                            StartAction = x => x.Start(),
                            StopAction = x => x.Stop(),
                            ContinueAction = x => x.Continue(),
                            PauseAction = x => x.Pause()
                        }
                };

            _serviceCoordinator.RegisterServices(services);

            _serviceCoordinator.Start();

            future.WaitUntilCompleted(5.Seconds()).ShouldBeTrue();
        }
    }
}