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
namespace Topshelf.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Magnum.Channels;
    using Magnum.Fibers;
    using Messages;
    using Shelving;

    public class DirectoryMonitor :
        IDisposable
    {
        FileSystemWatcher _fileSystemWatcher;
        readonly string _baseDir;
        readonly UntypedChannel _hostChannel;

        public DirectoryMonitor(string directory)
        {
            _baseDir = directory;
            _hostChannel = WellknownAddresses.GetHostChannelProxy();
        }

        public void Start()
        {
            // file system watcher will fail if directory isn't there, ensure it is
            if (!Directory.Exists(_baseDir))
                Directory.CreateDirectory(_baseDir);

            _fileSystemWatcher = new FileSystemWatcher(_baseDir)
                                     {
                                         IncludeSubdirectories = true,
                                         EnableRaisingEvents = true,
                                     };

            _fileSystemWatcher
                .GetEvents()
                .Select(e => GetChangedDirectory(e.EventArgs.FullPath))
                .BufferWithTime(TimeSpan.FromSeconds(3))
                .Where(e => e.Count() > 0)
                .Select(e => e.Distinct())
                .Subscribe(e =>
                {
                    e.ToList().ForEach(str => _hostChannel.Send(new FileSystemChange {ShelfName = str}));
                });
        }

        public void Stop()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.Dispose();
                _fileSystemWatcher = null;
            }
        }

        /// <summary>
        ///   Normalize the source of the event; we only care about the directory in question
        /// </summary>
        string GetChangedDirectory(string eventItem)
        {
            return eventItem.Substring(_baseDir.Length).Split(Path.DirectorySeparatorChar).Where(x => x.Length > 0).FirstOrDefault();
        }

        public void Dispose()
        {
            if (_fileSystemWatcher != null)
                _fileSystemWatcher.Dispose();
        }
    }

    public static class Extentions
    {
        public static IObservable<IEvent<FileSystemEventArgs>> GetEvents(this FileSystemWatcher fileSystemWatcher)
        {
            IObservable<IEvent<FileSystemEventArgs>> changed = Observable.FromEvent<FileSystemEventArgs>(fileSystemWatcher, "Changed");
            IObservable<IEvent<FileSystemEventArgs>> created = Observable.FromEvent<FileSystemEventArgs>(fileSystemWatcher, "Created");
            IObservable<IEvent<FileSystemEventArgs>> deleted = Observable.FromEvent<FileSystemEventArgs>(fileSystemWatcher, "Deleted");
            IObservable<IEvent<FileSystemEventArgs>> renamed = Observable.FromEvent<RenamedEventArgs>(fileSystemWatcher, "Renamed").Cast<IEvent<FileSystemEventArgs>>();

            return Observable.Merge(changed, created, deleted, renamed);
        }
    }
}