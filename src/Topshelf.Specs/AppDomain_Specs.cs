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
    using System.IO;
    using System.Threading;
    using Magnum.Extensions;
    using Magnum.TestFramework;
    using NUnit.Framework;
    using Shelving;
    using Topshelf.Configuration.Dsl;


    [TestFixture, Slow]
    public class AppDomain_Specs
    {
        [SetUp]
        public void Setup()
        {         
            if (Directory.Exists("Services"))
                Directory.Delete("Services", true);
            
            Directory.CreateDirectory("Services");
            var bobPath = Path.Combine("Services", "bob");
            Directory.CreateDirectory(bobPath);

            DirectoryMonitor_Specs.CopyFileToDir("TopShelf.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("TopShelf.Specs.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Magnum.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("log4net.dll", bobPath);
        }

        [TearDown]
        public void CleanUp()
        {
            Directory.Delete("Services", true);
        }

        [Test]
        public void Init_and_ready_service_in_seperate_app_domain()
        {
            using (var sm = new ShelfMaker())
            {
                using (var manualResetEvent = new ManualResetEvent(false))
                {
                    sm.OnShelfStateChanged += (sender, args) =>
                        {
                            if (args.ShelfName == "bob" && args.CurrentShelfState == ShelfState.Ready)
                                manualResetEvent.Set();
                        };

                    sm.MakeShelf("bob", typeof (AppDomain_Specs_Bootstrapper), GetType().Assembly.GetName());

                    manualResetEvent.WaitOne(20.Seconds());

                    sm.GetState("bob").ShouldEqual(ShelfState.Ready);
                }
            }
        }

        [Test, Slow]
        public void Stop_a_shelf()
        {
            using (var sm = new ShelfMaker())
            {
                var readyEvent = new ManualResetEvent(false);
                var stopEvent = new ManualResetEvent(false);

                sm.OnShelfStateChanged += (sender, args) =>
                {
                    if (args.ShelfName == "bob" && args.CurrentShelfState == ShelfState.Started)
                        readyEvent.Set();

                    if (args.ShelfName == "bob" && args.CurrentShelfState == ShelfState.Stopped)
                        stopEvent.Set();
                };

                sm.MakeShelf("bob", typeof(AppDomain_Specs_Bootstrapper), GetType().Assembly.GetName());

                readyEvent.WaitOne(60.Seconds());

                sm.GetState("bob").ShouldEqual(ShelfState.Started);

                sm.StopShelf("bob");

                stopEvent.WaitOne(60.Seconds());

                sm.GetState("bob").ShouldEqual(ShelfState.Stopped);
                
                //readyEvent.Dispose();
                //stopEvent.Dispose();
            }
        }
    }

    public class AppDomain_Specs_Bootstrapper :
        Bootstrapper<object>
    {
        public void InitializeHostedService(IServiceConfigurator<object> cfg)
        {
            cfg.HowToBuildService(serviceBuilder => new object());

            cfg.WhenStarted(a => { });
            cfg.WhenStopped(a => { });
        }
    }
}