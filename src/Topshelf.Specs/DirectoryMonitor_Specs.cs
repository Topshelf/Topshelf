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
    using System.IO;
    using System.Threading;
    using FileSystem;
    using Magnum.Channels;
    using Magnum.Extensions;
    using Magnum.Fibers;
    using Messages;
    using NUnit.Framework;
    using Shelving;

    [TestFixture]
    public class DirectoryMonitor_Specs
    {
        [Test]
        public void Identifies_changed_services_to_pipeline()
        {
            long count = 0;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            using (var manualResetEvent = new ManualResetEvent(false))
            {
                using (var dm = new DirectoryMonitor("."))
                {
                    using (var myChannel = new WcfUntypedChannelAdapter(new SynchronousFiber(), WellknownAddresses.HostAddress, "topshelf.host"))
                    {
                        dm.Start();

                        myChannel.Subscribe(sc => sc.Consume<FileSystemChange>()
                                                      .Using(fsc =>
                                                          {
                                                              var localCount = Interlocked.Increment(ref count);
                                                              Console.WriteLine(fsc.ServiceId);
                                                              if (localCount % 2 == 0)
                                                                manualResetEvent.Set();
                                                          }));

                        Console.WriteLine(baseDir);
                        //Thread.Sleep(1.Seconds());
                        Console.WriteLine("-- Directories");
                        
                        Directory.CreateDirectory(Path.Combine(baseDir, "Service1"));
                        Directory.CreateDirectory(Path.Combine(baseDir, "Service2"));
                        
                        manualResetEvent.WaitOne(15.Seconds());
                        count.ShouldEqual(2);
                        manualResetEvent.Reset();
                        
                        Console.WriteLine("-- Files");
                        
                        File.AppendAllText(Path.Combine(baseDir, "Service1", "test.out"), "Testing stuff");
                        File.AppendAllText(Path.Combine(baseDir, "Service2", "test.out"), "Testing stuff");
                        File.AppendAllText(Path.Combine(baseDir, "Service1", "test2.out"), "Testing stuff");
                        
                        manualResetEvent.WaitOne(10.Seconds());
                        
                        Console.WriteLine("-- Done");
                        
                        count.ShouldEqual(4);
                    }
                }
            }
        }

        [Test]
        public void Can_start_up_new_service_in_ShelfMaker_from_filesystem_event()
        {
            using (var sm = new ShelfMaker())
            {
                var dwStarted = new ManualResetEvent(false);
                var bobStarted = new ManualResetEvent(false);

                sm.OnShelfStateChanged += (sender, args) =>
                {
                    if (args.ShelfName == "bob" && args.CurrentShelfState == ShelfState.Started)
                        bobStarted.Set();

                    if (args.ShelfName == "TopShelf.DirectoryWatcher" && args.CurrentShelfState == ShelfState.Started)
                        dwStarted.Set();
                };
                sm.MakeShelf("TopShelf.DirectoryWatcher", typeof(DirectoryMonitorBootstrapper));

                string bobDir = Path.Combine(".", "Services", "bob");

                dwStarted.WaitOne(30.Seconds());

                sm.GetState("TopShelf.DirectoryWatcher").ShouldEqual(ShelfState.Started);

                Directory.CreateDirectory(Path.Combine(".", "Services"));
                Directory.CreateDirectory(bobDir);

                CopyFileToDir("TopShelf.dll", bobDir);
                CopyFileToDir("TopShelf.Specs.dll", bobDir);
                CopyFileToDir("Magnum.dll", bobDir);
                CopyFileToDir("System.CoreEx.dll", bobDir);
                CopyFileToDir("System.Reactive.dll", bobDir);
                File.Copy("service.config", Path.Combine(bobDir, "bob.config"));
                
                //sm.MakeShelf("bob", typeof(AppDomain_Specs_Bootstrapper), GetType().Assembly.GetName());

                bobStarted.WaitOne(30.Seconds());

                sm.GetState("bob").ShouldEqual(ShelfState.Started);
            }
        }

        [TearDown, SetUp]
        public void CleanUp()
        {
            if (Directory.Exists("Service1"))
                Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Service1"), true);
            if (Directory.Exists("Service2"))
                Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Service2"), true);

            if (Directory.Exists("Services"))
                Directory.Delete(Path.Combine(".", "Services"), true);
        }

        public static void CopyFileToDir(string sourceFile, string dir)
        {
            File.Copy(sourceFile, Path.Combine(dir, Path.GetFileName(sourceFile)));
        }
    }
}