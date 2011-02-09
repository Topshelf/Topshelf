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
namespace Topshelf.HostConfigurators
{
	using System;
	using System.Collections.Generic;
	using System.ServiceProcess;
	using Builders;
	using Magnum.Extensions;


	public class HostConfiguratorImpl :
		HostConfigurator,
		HostConfiguration
	{
		readonly IList<Action> _postInstallActions = new List<Action>();
		readonly IList<Action> _preInstallActions = new List<Action>();
		Func<HostConfiguration, HostBuilder> _builderFactory;

		IList<HostBuilderConfigurator> _configurators = new List<HostBuilderConfigurator>();

		public HostConfiguratorImpl()
		{
			StartMode = ServiceStartMode.Automatic;

			ServiceName = "";
			DisplayName = "";
			Description = "";

			_builderFactory = DefaultBuilderFactory;
		}

		public string Username { get; private set; }
		public string Password { get; private set; }
		public ServiceStartMode StartMode { get; private set; }
		public ServiceAccount AccountType { get; private set; }
		public string Description { get; private set; }
		public string DisplayName { get; private set; }
		public string ServiceName { get; private set; }
		public string InstanceName { get; private set; }

		public void Validate()
		{
			if (DisplayName.IsEmpty() && ServiceName.IsEmpty())
				throw new HostConfigurationException("The service display name must be specified.");

			if (ServiceName.IsEmpty())
				throw new HostConfigurationException("The service name must be specified.");

			_configurators.Each(x => x.Validate());
		}

		public void SetDisplayName(string name)
		{
			DisplayName = name;
		}

		public void SetServiceName(string name)
		{
			ServiceName = name;
		}

		public void SetDescription(string description)
		{
			Description = description;
		}

		public void UseBuilder(Func<HostConfiguration, HostBuilder> builderFactory)
		{
			_builderFactory = builderFactory;
		}

		public void SetInstanceName(string instanceName)
		{
			InstanceName = instanceName;
		}

		public void AddConfigurator(HostBuilderConfigurator configurator)
		{
			_configurators.Add(configurator);
		}

		public void SetUsername(string username)
		{
			Username = username;
		}

		public void SetPassword(string password)
		{
			Password = password;
		}

		public void SetAccountType(ServiceAccount accountType)
		{
			AccountType = accountType;
		}

		static HostBuilder DefaultBuilderFactory(HostConfiguration configuration)
		{
			return new RunBuilder(configuration);
		}

		public Host CreateHost()
		{
			HostBuilder builder = _builderFactory(this);

			foreach (HostBuilderConfigurator configurator in _configurators)
				configurator.Configure(builder);

			return builder.Build();
		}
	}
}