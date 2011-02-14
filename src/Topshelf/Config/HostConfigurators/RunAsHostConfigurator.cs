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
	using System.ServiceProcess;
	using Builders;
	using Magnum.Extensions;


	public class RunAsHostConfigurator :
		HostBuilderConfigurator
	{
		readonly ServiceAccount _accountType;
		readonly string _password;
		readonly string _username;

		public RunAsHostConfigurator(string username, string password, ServiceAccount accountType)
		{
			_username = username;
			_password = password;
			_accountType = accountType;
		}

		public RunAsHostConfigurator(ServiceAccount accountType)
		{
			_username = "";
			_password = "";
			_accountType = accountType;
		}

		public void Validate()
		{
			if (_username.IsNotEmpty() && _password.IsNotEmpty() && _accountType != ServiceAccount.User)
			{
				throw new HostConfigurationException(
					"The service account must be a user account when a username and password are specified");
			}

			if (_accountType == ServiceAccount.User && (_username.IsEmpty() || _password.IsEmpty()))
				throw new HostConfigurationException("The username and password must be specified for a user account");
		}

		public HostBuilder Configure(HostBuilder builder)
		{
			builder.Match<InstallBuilder>(x => x.RunAs(_username, _password, _accountType));

			return builder;
		}
	}
}