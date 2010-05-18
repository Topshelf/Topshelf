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
    using System.Reflection;
    using System.Threading;
    using NUnit.Framework;
    using Shelving;
    using Topshelf.Configuration.Dsl;

    //a place to learn about app-domains
    [TestFixture]
    public class AppDomain_Specs
    {
        [Test]
        public void Start_and_ready_service_in_seperate_app_domain()
        {
            using (var sm = new ShelfMaker())
            {
                sm.MakeShelf("bob", typeof(AppDomain_Specs_Bootstrapper), GetType().Assembly.GetName());

                Thread.Sleep(5000);

                sm.GetState("bob").ShouldEqual(ShelfState.Ready);
            }
        }

        [Test]
        /*
         * Edge case: what if there's an exception in the construction of the Shelf object after an adapter is connected to the pipe?
         * It won't be released in Dispose because the object was never created, should we make it a two parter? 
         *   Activtor.CreateInstance(Shelf)
         *   Shelf.Init()
         * To avoid anything like that happening? A lot of the work that can fail in the constructor doesn't make me feel good
         */
        public void Incorrect_bootstrapper_type_fails_MakeShelf()
        {
            using (var sm = new ShelfMaker())
            {
                // anything that fails during the Activator.CreateInstance will be a TargetInvocationException; gotta check the child instance
                // should we unwrap that in the ShelfMaker?
                Assert.That(() => sm.MakeShelf("doh", typeof(string)), Throws.InstanceOf<TargetInvocationException>().And.Property("InnerException").InstanceOf<InvalidOperationException>());
            }
        }
    }

    public class AppDomain_Specs_Bootstrapper :
        Bootstrapper
    {
        public void InitializeHostedService<T>(IServiceConfigurator<T> cfg)
        {
            cfg.HowToBuildService(serviceBuilder => new object());

            cfg.WhenStarted(a => { });
            cfg.WhenStopped(a => { });
        }
    }
}