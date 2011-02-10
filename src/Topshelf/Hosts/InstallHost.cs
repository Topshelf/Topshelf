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
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ServiceProcess;
	using Configuration;
	using log4net;
	using Magnum.Extensions;
	using WindowsServiceCode;


	public class InstallHost :
		ServiceHost,
		Host
	{
		readonly ILog _log = LogManager.GetLogger("Topshelf.Hosts.InstallHost");

		IEnumerable<Action> _postInstallActions;
		IEnumerable<Action> _preInstallActions;

		public InstallHost(ServiceDescription description,
		                   ServiceStartMode startMode, IEnumerable<string> dependencies, Credentials credentials)
			: base(description, startMode, dependencies, credentials)
		{
		}

		public void Run()
		{
			if (WinServiceHelper.IsInstalled(Description.GetServiceName()))
			{
				string message = string.Format("The {0} service is already installed.", Description.GetServiceName());
				_log.Error(message);

				return;
			}

			if (!UserAccessControlUtil.IsAdministrator)
			{
				if (!UserAccessControlUtil.RerunAsAdministrator())
					_log.ErrorFormat("The {0} service can only be installed as an administrator", Description.GetServiceName());

				return;
			}

			Install();

			//WinServiceHelper.SetRecoveryOptions(_settings.ServiceName.FullName, _settings.ServiceRecoveryOptions);
		}

		void Install()
		{
			_log.DebugFormat("Attempting to install '{0}'", Description.GetServiceName());

			ExecutePreInstallActions();

			WithInstaller(ti => ti.Install(new Hashtable()));

			ExecutePostInstallActions();
		}

		void ExecutePreInstallActions()
		{
			_preInstallActions.Each(x => x());
		}

		void ExecutePostInstallActions()
		{
			_postInstallActions.Each(x => x());
		}
	}
}