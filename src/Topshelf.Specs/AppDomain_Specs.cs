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
	using System.IO;
	using System.Threading;
	using log4net;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using Model;
	using NUnit.Framework;
	using Shelving;
	using Topshelf.Configuration.Dsl;


	[TestFixture]
	[Slow]
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
			DirectoryMonitor_Specs.CopyFileToDir("log4net.dll", bobPath);
			DirectoryMonitor_Specs.CopyFileToDir("Stact.dll", bobPath);
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

		public void InitializeHostedService(IServiceConfigurator<object> cfg)
		{
			cfg.HowToBuildService(serviceBuilder => new object());

			cfg.WhenStarted(a => { Started.Release(); });
			cfg.WhenStopped(a => { Stopped.Release(); });
		}
	}
}