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
namespace Topshelf.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceProcess;
	using Configuration;
	using HostConfigurators;
	using Hosts;
	using Magnum.Extensions;


	public class UninstallBuilder :
		HostBuilder
	{
		readonly IList<string> _dependencies = new List<string>();
		readonly string _description;
		readonly string _displayName;
		readonly string _instanceName;
		readonly IList<Action> _postActions = new List<Action>();
		readonly IList<Action> _preActions = new List<Action>();
		readonly string _serviceName;
		Credentials _credentials;
		ServiceStartMode _startMode;

		public UninstallBuilder(HostConfiguration configuration)
		{
			_serviceName = configuration.ServiceName;
			_displayName = configuration.DisplayName.IsEmpty() ? configuration.ServiceName : configuration.DisplayName;
			_description = configuration.Description.IsEmpty() ? _displayName : configuration.Description;
			_instanceName = configuration.InstanceName;

			_credentials = new Credentials("", "", ServiceAccount.LocalSystem);
		}

		public Host Build()
		{
			return new UninstallHost(_serviceName, _instanceName, _displayName, _description, _startMode, _dependencies.ToArray(),
			                         _credentials);
		}

		public void Match<T>(Action<T> callback)
			where T : class, HostBuilder
		{
			if (this is T)
				callback(this as T);
		}

		public void BeforeUninstall(Action callback)
		{
			_preActions.Add(callback);
		}

		public void AfterUninstall(Action callback)
		{
			_postActions.Add(callback);
		}
	}
}