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
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.ServiceProcess;
	using Magnum.Extensions;
	using WindowsServiceCode;


	public abstract class AbstractInstallerHost
	{
		readonly Credentials _credentials;
		readonly IEnumerable<string> _dependencies;
		readonly ServiceDescription _description;
		readonly IEnumerable<Action> _postActions;
		readonly IEnumerable<Action> _preActions;
		readonly ServiceStartMode _startMode;
	    readonly bool _delayedAutoStart;

		protected AbstractInstallerHost(ServiceDescription description, ServiceStartMode startMode,
		                                IEnumerable<string> dependencies, Credentials credentials,
		                                IEnumerable<Action> preActions, IEnumerable<Action> postActions,
                                        bool sudo, bool delayedAutoStart)
		{
			_startMode = startMode;
			_postActions = postActions;
			_preActions = preActions;
			_credentials = credentials;
			_dependencies = dependencies;
			_description = description;
            _delayedAutoStart = delayedAutoStart;
			Sudo = sudo;
		}

		protected bool Sudo { get; private set; }

	    public ServiceDescription Description
		{
			get { return _description; }
		}

		protected void WithInstaller(Action<TransactedInstaller> callback)
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			ExecutePreActions();

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

			ExecutePostActions();
		}

		protected abstract void CustomizeInstaller(Installer installer);

		protected void ExecutePreActions()
		{
			_preActions.Each(x => x());
		}

		protected void ExecutePostActions()
		{
			_postActions.Each(x => x());
		}

		Installer CreateInstaller()
		{
			var installers = new Installer[]
				{
					ConfigureServiceInstaller(),
					ConfigureServiceProcessInstaller()
				};

			string arguments = " ";

			if (_description.InstanceName.IsNotEmpty())
				arguments += " -instance \"{0}\"".FormatWith(_description.InstanceName);

            if (_description.DisplayName.IsNotEmpty())
                arguments += " -displayname \"{0}\"".FormatWith(_description.DisplayName);

            if (_description.Name.IsNotEmpty())
                arguments += " -servicename \"{0}\"".FormatWith(_description.Name);

			var installer = new HostInstaller(_description, arguments, installers);

			return installer;
		}

		ServiceInstaller ConfigureServiceInstaller()
		{
			var installer = new ServiceInstaller
				{
					ServiceName = _description.GetServiceName(),
					Description = _description.Description,
					DisplayName = _description.DisplayName,
					ServicesDependedOn = _dependencies.ToArray(),
					StartType = _startMode,
                    DelayedAutoStart = _delayedAutoStart
				};

			CustomizeInstaller(installer);

			return installer;
		}

		ServiceProcessInstaller ConfigureServiceProcessInstaller()
		{
			var installer = new ServiceProcessInstaller
				{
					Username = _credentials.Username,
					Password = _credentials.Password,
					Account = _credentials.Account,
				};

			return installer;
		}
	}
}