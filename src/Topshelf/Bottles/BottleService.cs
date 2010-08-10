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
namespace Topshelf.Bottles
{
    using System;
    using System.Configuration;
    using System.IO;
    using Magnum.FileSystem;
    using Directory = Magnum.FileSystem.Directory;

    public class BottleService
    {
        IDisposable _cleanup;
        FileSystem _fs;
        BottleWatcher _watcher;

        public void Start()
        {
            //TODO: how to find the services dir
            //TODO: how to get the bottles dir
            //TODO: do we need a custom config?
            string baseDir = ConfigurationManager.AppSettings["BottlesDirectory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bottles");
            _fs = new DotNetFileSystem();
            Directory bottlesDir = _fs.GetDirectory(baseDir);
            _watcher = new BottleWatcher();
            _cleanup = _watcher.Watch(bottlesDir.Name.GetPath(), CopyToServices);
        }

        void CopyToServices(Directory obj)
        {
            string serviceName = obj.Name.GetName();
            Directory targetDir = _fs.GetDirectory("Services").GetChildDirectory(serviceName);
            obj.CopyTo(targetDir.Name);
        }

        public void Stop()
        {
            _watcher = null;
            _cleanup.Dispose();
        }
    }
}