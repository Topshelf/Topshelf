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
	using FileSystem;
	using Magnum.Channels;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Messages;
	using NUnit.Framework;
	using Shelving;


    [TestFixture, Slow]
	public class DirectoryMonitor_Specs
	{
		[TearDown]
		[SetUp]
		public void CleanUp()
		{
			if (Directory.Exists("Service1"))
				Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Service1"), true);
			if (Directory.Exists("Service2"))
				Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Service2"), true);

			if (Directory.Exists("Services"))
				Directory.Delete(Path.Combine(".", "Services"), true);
		}

		[Test]
		public void Can_start_up_new_service_in_ShelfMaker_from_filesystem_event()
		{
			using (var dwStarted = new ManualResetEvent(false))
			using (var bobStaring = new ManualResetEvent(false))
			using (var sm = new ShelfMaker())
			{
				sm.OnShelfStateChanged += (sender, args) =>
					{
						if (args.ShelfName == "bob" && args.CurrentShelfState == ShelfState.Starting)
							bobStaring.Set();

						if (args.ShelfName == "TopShelf.DirectoryWatcher" &&
						    args.CurrentShelfState == ShelfState.Started)
							dwStarted.Set();
					};

				// this needs to happen before we attach the file watcher
				string srvDir = Path.Combine(".", "Services");
				Directory.CreateDirectory(srvDir);

				//Console.WriteLine("Starting TopShelf.DirectoryWatcher");
				sm.MakeShelf("TopShelf.DirectoryWatcher", typeof(DirectoryMonitorBootstrapper));
				dwStarted.WaitOne(20.Seconds());
				sm.GetState("TopShelf.DirectoryWatcher").ShouldEqual(ShelfState.Started);
				//Console.WriteLine("TopShelf.DirectoryWatcher started");

				// This isn't in setup, because we want the events to fire off to generate the shelf
				//Console.WriteLine("Copying files...");
				string bobDir = Path.Combine(srvDir, "bob");
				Directory.CreateDirectory(bobDir);

				CopyFileToDir("TopShelf.dll", bobDir);
				CopyFileToDir("TopShelf.Specs.dll", bobDir);
				CopyFileToDir("Magnum.dll", bobDir);
				CopyFileToDir("log4net.dll", bobDir);
				File.Copy("service.config", Path.Combine(bobDir, "bob.config"));
				//Console.WriteLine("Files copied, waiting for bob to start.");

				// let the service automagically start; using 'Starting' to speed up test
				bobStaring.WaitOne(20.Seconds());

				(sm.GetState("bob") == ShelfState.Starting || sm.GetState("bob") == ShelfState.Started).ShouldBeTrue();
			}
		}

		[Test]
		public void Identifies_changed_services_to_pipeline()
		{
			long count = 0;
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			using (var manualResetEvent = new ManualResetEvent(false))
			using (var dm = new DirectoryMonitor("."))
			{
				var myChannel = new ChannelAdapter();
				using (WellknownAddresses.GetShelfMakerHost(myChannel))
				{
					dm.Start();

					myChannel.Connect(sc => sc.AddConsumerOf<FileSystemChange>()
					                        	.UsingConsumer(fsc =>
					                        		{
                                                        if (fsc.ShelfName.Equals("bottle", StringComparison.OrdinalIgnoreCase))
                                                            return; // gotta skip the bottle directory

					                        			long localCount = Interlocked.Increment(ref count);
					                        			//Console.WriteLine(fsc.ShelfName);
					                        			if (localCount%2 == 0)
					                        				manualResetEvent.Set();
					                        		}));

					//Console.WriteLine(baseDir);
					//Console.WriteLine("-- Directories");

					Directory.CreateDirectory(Path.Combine(baseDir, "Service1"));
					Directory.CreateDirectory(Path.Combine(baseDir, "Service2"));

					manualResetEvent.WaitOne(15.Seconds());
					count.ShouldEqual(2);
					manualResetEvent.Reset();

					//Console.WriteLine("-- Files");

                    File.AppendAllText(Path.Combine(baseDir, Path.Combine("Service1", "test.out")), "Testing stuff");
					File.AppendAllText(Path.Combine(baseDir, Path.Combine("Service2", "test.out")), "Testing stuff");
                    File.AppendAllText(Path.Combine(baseDir, Path.Combine("Service1", "test2.out")), "Testing stuff");

					manualResetEvent.WaitOne(10.Seconds());

					//Console.WriteLine("-- Done");

					count.ShouldEqual(4);
				}
			}
		}

		public static void CopyFileToDir(string sourceFile, string dir)
		{
			File.Copy(sourceFile, Path.Combine(dir, Path.GetFileName(sourceFile)));
		}
	}
}