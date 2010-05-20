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
namespace Topshelf.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class DirectoryMonitor :
        IDisposable
    {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public DirectoryMonitor(string directory)
        {
            _fileSystemWatcher = new FileSystemWatcher(directory)
                {
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true, 
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastWrite
                };

            _fileSystemWatcher
                .GetEvents()
                .Select(e => GetChangedDirectory(e.EventArgs.FullPath))
                .BufferWithTime(TimeSpan.FromSeconds(3))
                //.Where(e => e.Count() > 0)
                //.Select(e => e.Distinct())
                .Subscribe(e =>
                    {
                        e.ToList().ForEach(Console.WriteLine);
                    });
        }

        /// <summary>
        /// Normalize the source of the event; we only care about the directory in question
        /// </summary>
        private static string GetChangedDirectory(string eventItem)
        {
            return eventItem;
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }
    }

    public static class Extentions
    {
        public static IObservable<IEvent<FileSystemEventArgs>> GetEvents(this FileSystemWatcher fileSystemWatcher)
        {
            var changed = Observable.FromEvent<FileSystemEventArgs>(fileSystemWatcher, "Changed");
            var created = Observable.FromEvent<FileSystemEventArgs>(fileSystemWatcher, "Created");
            var deleted = Observable.FromEvent<FileSystemEventArgs>(fileSystemWatcher, "Deleted");
            var renamed = Observable.FromEvent<RenamedEventArgs>(fileSystemWatcher, "Renamed").Cast<IEvent<FileSystemEventArgs>>();

            return Observable.Merge(changed, created, deleted, renamed);
        }
    }
}