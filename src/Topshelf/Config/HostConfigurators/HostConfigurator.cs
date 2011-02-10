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
	using Builders;
	using Configurators;


	public interface HostConfigurator :
		Configurator
	{
		/// <summary>
		/// Specifies the name of the service as it should be displayed in the service control manager
		/// </summary>
		/// <param name="name"></param>
		void SetDisplayName(string name);

		/// <summary>
		/// Specifies the name of the service as it is registered in the service control manager
		/// </summary>
		/// <param name="name"></param>
		void SetServiceName(string name);

		/// <summary>
		/// Specifies the description of the service that is displayed in the service control manager
		/// </summary>
		/// <param name="description"></param>
		void SetDescription(string description);

		/// <summary>
		/// Specifies the service instance name that should be used when the service is registered
		/// </summary>
		/// <param name="instanceName"></param>
		void SetInstanceName(string instanceName);

		/// <summary>
		/// Specifies the builder factory to use when the service is invoked
		/// </summary>
		/// <param name="builderFactory"></param>
		void UseBuilder(Func<ServiceDescription, HostBuilder> builderFactory);

		/// <summary>
		/// Adds a a configurator for the host builder to the configurator
		/// </summary>
		/// <param name="configurator"></param>
		void AddConfigurator(HostBuilderConfigurator configurator);
	}
}