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
namespace Topshelf.Host
{
	using System;
	using System.IO;
	using System.Linq;
	using FileSystem;
	using log4net;
	using Shelving;


	public class TopshelfHostService
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Host");

		IShelfServiceController _controller;

		public void Start()
		{
			_controller = new ShelfServiceController();

			StartDirectoryMonitor();

			StartExistingServices();
		}

		void StartDirectoryMonitor()
		{
			_controller.CreateShelfService("TopShelf.DirectoryWatcher", typeof(DirectoryMonitorBootstrapper));
			//_shelfMaker = new ShelfMaker();
			//_shelfMaker.MakeShelf("TopShelf.DirectoryWatcher", typeof(DirectoryMonitorBootstrapper));
		}

		void StartExistingServices()
		{
			string serviceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services");

			Directory.GetDirectories(serviceDir)
				.ToList()
				.ConvertAll(Path.GetFileName)
				.ForEach(StartShelfService);

			//.ForEach(dir => _shelfMaker.MakeShelf(dir));
		}

		void StartShelfService(string directoryName)
		{
			try
			{
				_controller.CreateShelfService(directoryName);
			}
			catch (Exception ex)
			{
				_log.Error("The service {0} could not be created".FormatWith(directoryName), ex);
			}
		}

		public void Stop()
		{
			_controller.Dispose();
		}
	}
}