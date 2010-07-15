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
    using System.IO;
    using System.Linq;
    using Magnum.Channels;
    using Magnum.Extensions;
    using Magnum.Fibers;
    using Magnum.FileSystem;
    using Magnum.FileSystem.Events;
    using Messages;
    using Shelving;

    public class DirectoryMonitor :
        IDisposable
    {
        readonly string _baseDir;
        readonly UntypedChannel _hostChannel;
        ChannelAdapter _channel;
        Scheduler _scheduler;
        PollingFileSystemEventProducer _producer;

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

            _channel = new ChannelAdapter();
            FiberFactory fiberFactory = () => new SynchronousFiber();
            _scheduler = new TimerScheduler(fiberFactory());
            _producer = new PollingFileSystemEventProducer(_baseDir, _channel, _scheduler, fiberFactory(),
                                                           2.Minutes());

            _channel.Connect(config => config
                                           .AddConsumerOf<FileSystemEvent>()
                                           .BufferFor(3.Seconds())
                                           .Distinct(fsEvent => GetChangedDirectory(fsEvent.Path))
                                           .UsingConsumer(fsEvents => fsEvents.Keys.ToList().ForEach(key =>
                                               {
                                                   if (key == _baseDir)
                                                       return;
                                                   
                                                   _hostChannel.Send(new FileSystemChange
                                                       {
                                                           ShelfName = key
                                                       });
                                               })));
        }

        public void Stop()
        {
            if (_producer != null)
            {
                _producer.Dispose();
                _producer = null;
            }

            if (_scheduler != null)
            {
                _scheduler.Stop();
                _scheduler = null;
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
            if (_scheduler != null)
                _scheduler.Stop();

            if (_producer != null)
                _producer.Dispose();
        }
    }
}