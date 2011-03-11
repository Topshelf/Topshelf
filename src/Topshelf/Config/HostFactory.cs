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
	using HostConfigurators;
	using log4net;


	public static class HostFactory
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Host");

		public static Host New(Action<HostConfigurator> configure)
		{
			var configurator = new HostConfiguratorImpl();

			configure(configurator);

			configurator.ApplyCommandLine();

			configurator.Validate();

			return configurator.CreateHost();
		}

		public static void Run(Action<HostConfigurator> configure)
		{
			try
			{
			    _log.InfoFormat(".Net Runtime {0}", Environment.Version);
			    _log.InfoFormat("Topshelf v{0}", typeof(HostFactory).Assembly.GetName().Version);

				New(configure)
					.Run();
			}
			catch (Exception ex)
			{
				_log.Error("The service exited abnormally with an exception", ex);
			}
		}
	}
}