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
    using System;
    using Model;
    using NUnit.Framework;
    using TestObject;
    using Topshelf.Configuration.Dsl;

    [TestFixture]
    [Serializable] //because of the lambda
    public class IsolatedService_Specs
    {
        private IServiceController _serviceController;

        [SetUp]
        public void EstablishContext()
        {
            
            

            var c = new IsolatedServiceConfigurator<TestService>();
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());
            //c.WhenPaused(s => { _wasPaused = true; }); //need to change these
            //c.WhenContinued(s => { _wasContinued = true; }); //need to change these
            c.HowToBuildService((name)=>
                                   {
                                       return new TestService();
                                   });

            _serviceController = c.Create();
            _serviceController.Start();

        }

        [Test]
        public void Should_start()
        {
            _serviceController.State
                .ShouldEqual(ServiceState.Started);
        }

        [Test]
        public void Should_pause()
        {
            _serviceController.Pause();
            
            _serviceController.State
                .ShouldEqual(ServiceState.Paused);

            //probably can't do this as the cross domain
            //_wasPaused
               // .ShouldBeTrue();
        }

        [Test]
        public void Should_continue()
        {
            _serviceController.Continue();

            _serviceController.State
                .ShouldEqual(ServiceState.Started);

            //probably can't do this as the cross domain
            //_wasContinued
            //    .ShouldBeTrue();
        }

        [Test]
        public void Should_stop()
        {
            _serviceController.Stop();

			Assert.Throws<AppDomainUnloadedException>(()=>
				{
					//this throws the exception
					_serviceController.State
						.ShouldEqual(ServiceState.Stopped);
				});
        }

        [Test]
        public void Should_expose_contained_type()
        {
            _serviceController.ServiceType
                .ShouldEqual(typeof(TestService));
        }

        //TODO: state transition tests
    }
}