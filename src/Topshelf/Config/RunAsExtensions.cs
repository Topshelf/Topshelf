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
namespace Topshelf
{
	using System;
	using System.ServiceProcess;
	using HostConfigurators;
	using Internal;


	public static class RunAsExtensions
	{
		public static HostConfigurator RunAs([NotNull] this HostConfigurator configurator, string username, string password)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			var runAsConfigurator = new RunAsHostConfigurator(username, password, ServiceAccount.User);

			configurator.AddConfigurator(runAsConfigurator);

			return configurator;
		}

		public static HostConfigurator RunAsNetworkService([NotNull] this HostConfigurator configurator)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			var runAsConfigurator = new RunAsHostConfigurator(ServiceAccount.NetworkService);

			configurator.AddConfigurator(runAsConfigurator);

			return configurator;
		}

		public static HostConfigurator RunAsLocalSystem([NotNull] this HostConfigurator configurator)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			var runAsConfigurator = new RunAsHostConfigurator(ServiceAccount.LocalSystem);

			configurator.AddConfigurator(runAsConfigurator);

			return configurator;
		}

		public static HostConfigurator RunAsLocalService([NotNull] this HostConfigurator configurator)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			var runAsConfigurator = new RunAsHostConfigurator(ServiceAccount.LocalService);

			configurator.AddConfigurator(runAsConfigurator);

			return configurator;
		}
	}
}