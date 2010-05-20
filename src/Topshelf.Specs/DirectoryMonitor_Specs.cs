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
    using Magnum.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class DirectoryMonitor_Specs
    {
        [Test]
        public void Identifies_changed_files()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            using (var dm = new DirectoryMonitor(baseDir))
            {
                Console.WriteLine(baseDir);
                Thread.Sleep(100);
                Console.WriteLine("-- Directories");
                Directory.CreateDirectory(Path.Combine(baseDir, "Service1"));
                Directory.CreateDirectory(Path.Combine(baseDir, "Service2"));
                Thread.Sleep(8.Seconds());
                Console.WriteLine("-- Files");
                File.AppendAllText(Path.Combine(baseDir, "Service1", "test.out"), "Testing stuff");
                File.AppendAllText(Path.Combine(baseDir, "Service2", "test.out"), "Testing stuff");
                File.AppendAllText(Path.Combine(baseDir, "Service1", "test2.out"), "Testing stuff");
                Thread.Sleep(12.Seconds());
            }

            Directory.Delete(Path.Combine(baseDir, "Service1"), true);
            Directory.Delete(Path.Combine(baseDir, "Service2"), true);
        }
    }
}