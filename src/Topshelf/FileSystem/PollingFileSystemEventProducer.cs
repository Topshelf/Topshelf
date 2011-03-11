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
	using System.Security.Cryptography;
	using Magnum.FileSystem.Events;
	using Magnum.Extensions;
	using Magnum.FileSystem.Internal;
	using Stact;


	public class PollingFileSystemEventProducer :
		IDisposable
	{
		readonly UntypedChannel _channel;
		readonly TimeSpan _checkInterval;
		readonly ChannelConnection _connection;
		readonly string _directory;
		readonly Fiber _fiber;
		readonly FileSystemEventProducer _fileSystemEventProducer;
		readonly Dictionary<string, Guid> _hashes;
		readonly Scheduler _scheduler;
		bool _disposed;
		ScheduledOperation _scheduledAction;

		/// <summary>
		/// Creates a PollingFileSystemEventProducer
		/// </summary>		
		/// <param name="directory">The directory to watch</param>
		/// <param name="channel">The channel where events should be sent</param>
		/// <param name="scheduler">Event scheduler</param>
		/// <param name="fiber">Fiber to schedule on</param>
		/// <param name="checkInterval">The maximal time between events or polls on a given file</param>
		public PollingFileSystemEventProducer(string directory, UntypedChannel channel, Scheduler scheduler, Fiber fiber,
											  TimeSpan checkInterval) :
			this(directory, channel, scheduler, fiber, checkInterval, true)
		{
		}

		/// <summary>
		/// Creates a PollingFileSystemEventProducer
		/// </summary>		
		/// <param name="directory">The directory to watch</param>
		/// <param name="channel">The channel where events should be sent</param>
		/// <param name="scheduler">Event scheduler</param>
		/// <param name="fiber">Fiber to schedule on</param>
		/// <param name="checkInterval">The maximal time between events or polls on a given file</param>
		/// <param name="checkSubDirectory">Indicates if subdirectorys will be checked or ignored</param>
		public PollingFileSystemEventProducer(string directory, UntypedChannel channel, Scheduler scheduler, Fiber fiber,
											  TimeSpan checkInterval, bool checkSubDirectory)
		{
			_directory = directory;
			_channel = channel;
			_fiber = fiber;
			_hashes = new Dictionary<string, Guid>();
			_scheduler = scheduler;
			_checkInterval = checkInterval;

			_scheduledAction = scheduler.Schedule(3.Seconds(), _fiber, HashFileSystem);

			ChannelAdapter myChannel = new ChannelAdapter();

			_connection = myChannel.Connect(connectionConfigurator =>
			{
				connectionConfigurator.AddConsumerOf<FileSystemChanged>().UsingConsumer(HandleFileSystemChangedAndCreated);
				connectionConfigurator.AddConsumerOf<FileSystemCreated>().UsingConsumer(HandleFileSystemChangedAndCreated);
				connectionConfigurator.AddConsumerOf<FileSystemRenamed>().UsingConsumer(HandleFileSystemRenamed);
				connectionConfigurator.AddConsumerOf<FileSystemDeleted>().UsingConsumer(HandleFileSystemDeleted);
			});

			_fileSystemEventProducer = new FileSystemEventProducer(directory, myChannel, checkSubDirectory);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void HandleFileSystemChangedAndCreated(FileSystemEvent fileEvent)
		{
			HandleHash(fileEvent.Path, GenerateHashForFile(fileEvent.Path));
		}

		void HandleFileSystemRenamed(FileSystemRenamed fileEvent)
		{
			HandleFileSystemChangedAndCreated(fileEvent);
			HandleFileSystemDeleted(new FileSystemDeletedImpl(fileEvent.OldName, fileEvent.OldPath));
		}

		void HandleFileSystemDeleted(FileSystemEvent fileEvent)
		{
			RemoveHash(fileEvent.Path);
		}

		void HandleHash(string key, Guid newHash)
		{
			if (string.IsNullOrEmpty(key))
				return;

			if (_hashes.ContainsKey(key))
			{
				if (_hashes[key] != newHash)
				{
					_hashes[key] = newHash;
					_channel.Send(new FileChangedImpl(Path.GetFileName(key), key));
				}
			}
			else
			{
				_hashes.Add(key, newHash);
				_channel.Send(new FileCreatedImpl(Path.GetFileName(key), key));
			}
		}

		void RemoveHash(string key)
		{
			_hashes.Remove(key);
			_channel.Send(new FileSystemDeletedImpl(Path.GetFileName(key), key));
		}

		void HashFileSystem()
		{
			try
			{
				Dictionary<string, Guid> newHashes = new Dictionary<string, Guid>();

				ProcessDirectory(newHashes, _directory);

				// process all the new hashes found
				newHashes.ToList().ForEach(x => HandleHash(x.Key, x.Value));

				// remove any hashes we couldn't process
				_hashes.Where(x => !newHashes.ContainsKey(x.Key)).ToList().ConvertAll(x => x.Key).ForEach(RemoveHash);
			}
			finally
			{
				_scheduledAction = _scheduler.Schedule(_checkInterval, _fiber, HashFileSystem);
			}
		}

		void ProcessDirectory(Dictionary<string, Guid> hashes, string baseDirectory)
		{
			string[] files = System.IO.Directory.GetFiles(baseDirectory);

			foreach (string file in files)
			{
				string fullFileName = Path.Combine(baseDirectory, file);
				hashes.Add(fullFileName, GenerateHashForFile(fullFileName));
			}

			System.IO.Directory.GetDirectories(baseDirectory).ToList().Each(dir => ProcessDirectory(hashes, dir));
		}

		static Guid GenerateHashForFile(string file)
		{
			try
			{
				string hashValue;
				using (FileStream f = File.OpenRead(file))
				using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
				{
					byte[] fileHash = md5.ComputeHash(f);

					hashValue = BitConverter.ToString(fileHash).Replace("-", "");
				}

				return new Guid(hashValue);
			}
			catch (Exception)
			{
				// chew up exception and say empty hash
				// can we do something more interesting than this?
				return Guid.Empty;
			}
		}

		~PollingFileSystemEventProducer()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				_scheduledAction.Cancel();
				_connection.Dispose();
				_fileSystemEventProducer.Dispose();
			}

			_disposed = true;
		}
	}
}