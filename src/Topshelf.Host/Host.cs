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
	using System.Linq;
	using System.Threading;
	using FileSystem;
	using Messages;
	using Model;
	using Shelving;


	public class Host
	{
		public const string DefaultServiceName = "Topshelf.Host";

		readonly IServiceCoordinator _coordinator;

		public Host(IServiceCoordinator coordinator)
		{
			_coordinator = coordinator;
		}

		public void Start()
		{
			CreateDirectoryMonitor();
		}

		void CreateDirectoryMonitor()
		{
			_coordinator.Send(new CreateShelfService("TopShelf.DirectoryMonitor",
			                                         ShelfType.Internal,
			                                         typeof(DirectoryMonitorBootstrapper)));
		}

		public void Stop()
		{
			while (_coordinator.GetRunningServices().Count() > 1)
				Thread.Sleep(100);
		}
	}
}