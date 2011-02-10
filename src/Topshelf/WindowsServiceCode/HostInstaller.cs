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
namespace Topshelf.WindowsServiceCode
{
	using System.Collections;
	using System.Configuration.Install;
	using log4net;
	using Microsoft.Win32;


	public class HostInstaller :
		Installer
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.WindowsServiceCode.HostInstaller");
		readonly ServiceDescription _description;
		readonly string _arguments;
		readonly Installer[] _installers;

		public HostInstaller(ServiceDescription description, string arguments, Installer[] installers)
		{
			_installers = installers;
			_arguments = arguments;
			_description = description;
		}

		public override void Install(IDictionary stateSaver)
		{
			Installers.AddRange(_installers);

			if (_log.IsInfoEnabled)
				_log.InfoFormat("Installing {0} service", _description.Name);

			base.Install(stateSaver);

			if (_log.IsDebugEnabled)
				_log.Debug("Opening Registry");

			using (RegistryKey system = Registry.LocalMachine.OpenSubKey("System"))
			using (RegistryKey currentControlSet = system.OpenSubKey("CurrentControlSet"))
			using (RegistryKey services = currentControlSet.OpenSubKey("Services"))
			using (RegistryKey service = services.OpenSubKey(_description.GetServiceName(), true))
			{
				service.SetValue("Description", _description.Description);

				var imagePath = (string)service.GetValue("ImagePath");

				_log.DebugFormat("Service path: {0}", imagePath);

				imagePath += _arguments;

				_log.DebugFormat("Image path: {0}", imagePath);

				service.SetValue("ImagePath", imagePath);
			}

			if (_log.IsDebugEnabled)
				_log.Debug("Closing Registry");
		}

		public override void Uninstall(IDictionary savedState)
		{
			Installers.AddRange(_installers);
			if (_log.IsInfoEnabled)
				_log.InfoFormat("Uninstalling {0} service", _description.Name);

			base.Uninstall(savedState);
		}
	}
}