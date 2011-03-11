// Copyright 2007-2011 The Apache Software Foundation.
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
	using FileSystem;
	using Magnum.Extensions;
	using Magnum.FileSystem;
	using Magnum.FileSystem.Events;
	using Magnum.FileSystem.Zip;
	using Stact;
	using Stact.Internal;

	public class BottleWatcher
	{
		Action<Directory> _actionToTake;

		public IDisposable Watch(string directoryToWatch, Action<Directory> actionToTake)
		{
			_actionToTake = actionToTake;

		    Func<Fiber> fiberFactory = () => new SynchronousFiber();

			if (!System.IO.Directory.Exists(directoryToWatch))
				System.IO.Directory.CreateDirectory(directoryToWatch);

			var eventChannel = new ChannelAdapter();
			eventChannel.Connect(x => x.AddConsumerOf<FileCreated>().UsingConsumer(ProcessNewFile));

			Scheduler scheduler = new TimerScheduler(fiberFactory());
			var watcher = new PollingFileSystemEventProducer(directoryToWatch, eventChannel, scheduler, fiberFactory(),
			                                              1.Seconds());

			return watcher;
		}

		void ProcessNewFile(FileCreated message)
		{
			if (message.Path.EndsWith("bottle"))
			{
				Directory dir = new ZipFileDirectory(PathName.GetPathName(message.Path));
				try
				{
					_actionToTake(dir);
				}
				catch (Exception ex)
				{
					string msg = "There was an error processing the bottle '{0}'".FormatWith(message.Path);
					throw new BottleException(msg, ex);
				}
			}
		}


	}
}