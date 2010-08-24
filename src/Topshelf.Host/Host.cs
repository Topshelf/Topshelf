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
	using Shelving;


	public class Host
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Host");

		IServiceCoordinator _coordinator;

		public void Start()
		{
			_coordinator = new ServiceCoordinator();

			CreateDirectoryMonitor();

			CreateExistingServices();
		}

		void CreateDirectoryMonitor()
		{
			_coordinator.CreateShelfService("TopShelf.DirectoryMonitor", typeof(DirectoryMonitorBootstrapper));
		}

		void CreateExistingServices()
		{
			string serviceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services");

			Directory.GetDirectories(serviceDir)
				.ToList()
				.ConvertAll(Path.GetFileName)
				.ForEach(CreateShelfService);
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
			_coordinator.Dispose();
		}
	}
}