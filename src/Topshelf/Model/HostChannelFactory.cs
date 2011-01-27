// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Topshelf.Model
{
	using System;
	using System.Diagnostics;
	using Magnum.Extensions;
	using Stact;
	using Stact.Configuration;


	public static class HostChannelFactory
	{
		static int GetPid()
		{
			return Process.GetCurrentProcess().Id;
		}

		static Uri GetServiceUri()
		{
			return new Uri("net.pipe://localhost/topshelf_{0}".FormatWith(GetPid()));
		}

		public static HostChannel CreateShelfControllerHost(UntypedChannel controllerChannel, string serviceName)
		{
			Uri address = GetServiceUri().AppendPath("controller").AppendPath(serviceName);
			string pipeName = "{0}/{1}".FormatWith(GetPid(), serviceName);

			return new HostChannel(controllerChannel, address, pipeName);
		}

		public static HostChannel CreateShelfHost(string serviceName, Action<ConnectionConfigurator> cfg)
		{
			Uri address = GetServiceUri().AppendPath("shelf").AppendPath(serviceName);
			string pipeName = "{0}/{1}".FormatWith(GetPid(), serviceName);

			return new HostChannel(address, pipeName, cfg);
		}
	}
}