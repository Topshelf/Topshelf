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
	using System.Collections.Generic;
	using System.Configuration.Install;
	using System.Linq;
	using System.Reflection;
	using System.ServiceProcess;
	using Configuration;
	using Magnum.Extensions;
	using WindowsServiceCode;


	public abstract class ServiceHost
	{
		readonly Credentials _credentials;
		readonly IEnumerable<string> _dependencies;
		readonly string _description;
		readonly string _displayName;
		readonly string _instanceName;
		readonly ServiceStartMode _startMode;

		protected ServiceHost(string serviceName, string instanceName, string displayName, string description,
		                      ServiceStartMode startMode, IEnumerable<string> dependencies, Credentials credentials)
		{
			ServiceName = serviceName;
			_startMode = startMode;
			_instanceName = instanceName;
			_credentials = credentials;
			_dependencies = dependencies;
			_description = description;
			_displayName = displayName;
		}

		public string ServiceName { get; private set; }

		protected void WithInstaller(Action<TransactedInstaller> callback)
		{
			using (Installer installer = CreateInstaller())
			using (var ti = new TransactedInstaller())
			{
				ti.Installers.Add(installer);

				Assembly assembly = Assembly.GetEntryAssembly();
				if (assembly == null)
					throw new HostException("Assembly.GetEntryAssembly() is null for some reason.");

				string path = string.Format("/assemblypath={0}", assembly.Location);
				string[] commandLine = {path};

				var context = new InstallContext(null, commandLine);
				ti.Context = context;

				callback(ti);
			}
		}

		protected Installer CreateInstaller()
		{
			var installers = new Installer[]
				{
					ConfigureServiceInstaller(),
					ConfigureServiceProcessInstaller()
				};

			string arguments = " ";

			if (_instanceName.IsNotEmpty())
				arguments += " -instance:{0}".FormatWith(_instanceName);

			var installer = new HostInstaller(ServiceName, _description, arguments, installers);

			return installer;
		}

		ServiceInstaller ConfigureServiceInstaller()
		{
			var installer = new ServiceInstaller
				{
					ServiceName = ServiceName,
					Description = _description,
					DisplayName = _displayName,
					ServicesDependedOn = _dependencies.ToArray(),
					StartType = _startMode,
				};

			return installer;
		}

		ServiceProcessInstaller ConfigureServiceProcessInstaller()
		{
			var installer = new ServiceProcessInstaller
				{
					Account = ServiceAccount.LocalSystem
				};

			if (_credentials != null)
			{
				installer.Username = _credentials.Username;
				installer.Password = _credentials.Password;
				installer.Account = _credentials.Account;
			}

			return installer;
		}
	}
}