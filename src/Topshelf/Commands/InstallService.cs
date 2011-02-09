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
namespace Topshelf.Commands
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Reflection;
	using Configuration;
	using log4net;
	using WindowsServiceCode;


	public class InstallService :
		Command
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Commands.InstallService");

		readonly WinServiceSettings _settings;
		readonly string _commandLine;

		public InstallService(WinServiceSettings settings, string commandLine)
		{
			_settings = settings;
			_commandLine = commandLine;
		}

		#region Command Members

		public ServiceActionNames Name
		{
			get { return ServiceActionNames.Install; }
		}

		public void Execute()
		{
			if (WinServiceHelper.IsInstalled(_settings.ServiceName.FullName))
			{
				string message = string.Format("The {0} service has already been installed.", _settings.ServiceName.FullName);
				_log.Error(message);

				return;
			}

			if (!UserAccessControlUtil.IsAdministrator)
			{
				if ( Environment.OSVersion.Version.Major == 6)
				{
					var startInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, _commandLine);
					startInfo.Verb = "runas";
					startInfo.UseShellExecute = true;
					startInfo.CreateNoWindow = true;

					try
					{
						Process process = Process.Start(startInfo);
						process.WaitForExit();

						return;
					}
					catch (Win32Exception ex)
					{
						_log.Debug("Process Start Exception", ex);
					}
				}

				_log.ErrorFormat("The {0} service can only be installed as an administrator", _settings.ServiceName.FullName);
				return;
			}

			var installer = new HostServiceInstaller(_settings);
			WinServiceHelper.SetRecoveryOptions(_settings.ServiceName.FullName, _settings.ServiceRecoveryOptions);
			WinServiceHelper.Register(_settings.ServiceName.FullName, installer, _settings.AfterInstallAction);
		}

		#endregion
	}
}