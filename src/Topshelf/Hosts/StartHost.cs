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
namespace Topshelf.Hosts
{
	using log4net;
	using Windows;


	public class StartHost :
		Host
	{
		readonly ILog _log = LogManager.GetLogger("Topshelf.Hosts.StartHost");
		readonly Host _wrappedHost;

		public StartHost(ServiceDescription description, Host wrappedHost)
		{
			_wrappedHost = wrappedHost;
			Description = description;
		}

		public ServiceDescription Description { get; private set; }

		public void Run()
		{
			if (!WindowsUserAccessControl.IsAdministrator)
			{
				if (!WindowsUserAccessControl.RerunAsAdministrator())
					_log.ErrorFormat("The {0} service can only be started by an administrator", Description.GetServiceName());

				return;
			}

			_wrappedHost.Run();

			if (!WindowsServiceControlManager.IsInstalled(Description.GetServiceName()))
			{
				_log.ErrorFormat("The {0} service is not installed.", Description.GetServiceName());
				return;
			}

			_log.DebugFormat("Attempting to start '{0}'", Description.GetServiceName());

			WindowsServiceControlManager.Start(Description.GetServiceName());

			_log.InfoFormat("The {0} service has been started.", Description.GetServiceName());
		}
	}
}