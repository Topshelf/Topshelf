// Copyright 2007-2012 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// his file except in compliance with the License. You may obtain a copy of the 
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
    using System.IO;
    using System.Threading;
    using Configuration.Dsl;
    using Magnum.Extensions;
    using Messages;
    using Model;
    using NUnit.Framework;
    using ServiceConfigurators;
    using Shelving;
    using log4net;


    [TestFixture]
    [Explicit]
    public class Using_the_shelf_service_controller_to_start_a_service
    {
        [SetUp]
        public void Setup()
        {
            if (Directory.Exists("Services"))
                Directory.Delete("Services", true);

            Directory.CreateDirectory("Services");
            string bobPath = Path.Combine("Services", "bob");
            Directory.CreateDirectory(bobPath);

            DirectoryMonitor_Specs.CopyFileToDir("TopShelf.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("TopShelf.Specs.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Magnum.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Magnum.FileSystem.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Ionic.Zip.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Newtonsoft.Json.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("System.Threading.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("log4net.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Stact.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Stact.serverframework.dll", bobPath);
            DirectoryMonitor_Specs.CopyFileToDir("Spark.dll", bobPath);
        }

        [TearDown]
        public void CleanUp()
        {
            //Directory.Delete("Services", true);
        }

        [Test]
        public void Should_start_the_shelf_in_the_separate_app_domain()
        {
            _log.Debug("Starting up the controller");

            using (var coordinator = new Model.ServiceCoordinator())
            {
                coordinator.Start();
                coordinator.Send(new CreateShelfService("bob", ShelfType.Folder, typeof(TestAppDomainBootsrapper)));

                TestAppDomainBootsrapper.Started.WaitOne(20.Seconds()).ShouldBeTrue();
            }
        }

        static readonly ILog _log = LogManager.GetLogger(typeof(Using_the_shelf_service_controller_to_start_a_service));
    }


    public class TestAppDomainBootsrapper :
        Bootstrapper<object>
    {
        [ThreadStatic]
        static Semaphore _started;

        [ThreadStatic]
        static Semaphore _stopped;

        public static Semaphore Started
        {
            get
            {
                if (_started == null)
                    _started = new Semaphore(0, 100, "TestAppDomainBootstrapperSemaphore");

                return _started;
            }
        }

        public static Semaphore Stopped
        {
            get
            {
                if (_stopped == null)
                    _stopped = new Semaphore(0, 100, "TestAppDomainBootstrapperSemaphore");

                return _stopped;
            }
        }

        public void InitializeHostedService(ServiceConfigurator<object> cfg)
        {
            cfg.ConstructUsing(s => new object());

            cfg.WhenStarted(a => { Started.Release(); });
            cfg.WhenStopped(a => { Stopped.Release(); });
        }
    }
}