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
    using System.IO;
    using System.Threading;
    using Magnum.Extensions;
    using NUnit.Framework;
    using Shelving;
    using Topshelf.Configuration.Dsl;

    //a place to learn about app-domains
    [TestFixture]
    public class AppDomain_Specs
    {
        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory("Services");
            Directory.CreateDirectory(Path.Combine("Services", "bob"));

            DirectoryMonitor_Specs.CopyFileToDir("TopShelf.dll", Path.Combine("Services", "bob"));
            DirectoryMonitor_Specs.CopyFileToDir("TopShelf.Specs.dll", Path.Combine("Services", "bob"));
            DirectoryMonitor_Specs.CopyFileToDir("Magnum.dll", Path.Combine("Services", "bob"));
            DirectoryMonitor_Specs.CopyFileToDir("System.CoreEx.dll", Path.Combine("Services", "bob"));
            DirectoryMonitor_Specs.CopyFileToDir("System.Reactive.dll", Path.Combine("Services", "bob"));
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
                sm.MakeShelf("bob", typeof(AppDomain_Specs_Bootstrapper), GetType().Assembly.GetName());

                // this takes too long currently... can we do better?
                Thread.Sleep(20.Seconds());

                sm.GetState("bob").ShouldEqual(ShelfState.Ready);

                Thread.Sleep(1.Seconds());
            }
        }

        [Test]
        public void Stop_a_shelf()
        {
            using (var sm = new ShelfMaker())
            {
                sm.MakeShelf("bob", typeof(AppDomain_Specs_Bootstrapper), GetType().Assembly.GetName());

                Thread.Sleep(5.Seconds());

                sm.GetState("bob").ShouldEqual(ShelfState.Ready);
                sm.StopShelf("bob");

                Thread.Sleep(3.Seconds());

                sm.GetState("bob").ShouldEqual(ShelfState.Stopped);
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