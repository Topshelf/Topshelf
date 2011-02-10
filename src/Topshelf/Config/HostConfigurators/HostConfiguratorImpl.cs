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
	using Builders;
	using Magnum.Extensions;


	public class HostConfiguratorImpl :
		HostConfigurator
	{
		readonly IList<Action> _postInstallActions = new List<Action>();
		readonly IList<Action> _preInstallActions = new List<Action>();
		Func<ServiceDescription, HostBuilder> _builderFactory;

		readonly WindowsServiceDescription _description;

		public WindowsServiceDescription Description
		{
			get { return _description; }
		}

		IList<HostBuilderConfigurator> _configurators = new List<HostBuilderConfigurator>();

		public HostConfiguratorImpl()
		{
			_description = new WindowsServiceDescription();

			_builderFactory = DefaultBuilderFactory;
		}

		public void Validate()
		{
			if (_description.DisplayName.IsEmpty() && _description.Name.IsEmpty())
				throw new HostConfigurationException("The service display name must be specified.");

			if (_description.Name.IsEmpty())
				throw new HostConfigurationException("The service name must be specified.");

			_configurators.Each(x => x.Validate());
		}

		public void SetDisplayName(string name)
		{
			_description.DisplayName = name;
		}

		public void SetServiceName(string name)
		{
			_description.Name = name;
		}

		public void SetDescription(string description)
		{
			_description.Description = description;
		}

		public void SetInstanceName(string instanceName)
		{
			_description.InstanceName = instanceName;
		}

		public void UseBuilder(Func<ServiceDescription, HostBuilder> builderFactory)
		{
			_builderFactory = builderFactory;
		}

		public void AddConfigurator(HostBuilderConfigurator configurator)
		{
			_configurators.Add(configurator);
		}

		static HostBuilder DefaultBuilderFactory(ServiceDescription description)
		{
			return new RunBuilder(description);
		}

		public Host CreateHost()
		{
			HostBuilder builder = _builderFactory(_description);

			foreach (HostBuilderConfigurator configurator in _configurators)
				configurator.Configure(builder);

			return builder.Build();
		}
	}
}