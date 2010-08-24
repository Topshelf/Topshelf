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
namespace Topshelf
{
	using System;
	using System.IO;
	using System.Linq;
	using FileSystem;
	using log4net;
	using Model;


	public class Host
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Host");
		readonly IServiceCoordinator _coordinator;

		public Host(IServiceCoordinator coordinator)
		{
			_coordinator = coordinator;
		}

		public void Start()
		{
			CreateDirectoryMonitor();

			CreateExistingServices();
		}

		void CreateDirectoryMonitor()
		{
			_coordinator.CreateShelfService("TopShelf.DirectoryMonitor", typeof(DirectoryMonitorBootstrapper));
		}

		void CreateExistingServices()
		{
			var serviceDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services"));

			if (serviceDirectory.Exists)
			{
				Directory.GetDirectories(serviceDirectory.FullName)
					.ToList()
					.ConvertAll(Path.GetFileName)
					.ForEach(CreateShelfService);
			}
			else
			{
				_log.WarnFormat("The services folder does not exist");
			}
		}

		void CreateShelfService(string directoryName)
		{
			try
			{
				_coordinator.CreateShelfService(directoryName);
			}
			catch (Exception ex)
			{
				_log.Error("The service {0} could not be created".FormatWith(directoryName), ex);
			}
		}

		public void Stop()
		{
			_log.DebugFormat("Stop called on Host");
		}
	}
}