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
namespace Topshelf
{
	using System.ServiceProcess;
	using Commands;
	using Configuration;


	class InstallCommand : Command
	{
		readonly WinServiceSettings _settings;
		readonly string _commandLine;

		public InstallCommand(WinServiceSettings settings, string commandLine, string instance, string username,
		                      string password)
		{
			_settings = settings;
			_commandLine = commandLine;

			if (username != null && password != null)
				_settings.Credentials = new Credentials(username, password, ServiceAccount.User);

			if (instance != null)
			{
				_settings.ServiceName = new ServiceName(_settings.ServiceName.Name, instance);
			}
		}

		public ServiceActionNames Name
		{
			get { return ServiceActionNames.Install; }
		}

		public void Execute()
		{
			new InstallService(_settings, _commandLine).Execute();
		}
	}
}