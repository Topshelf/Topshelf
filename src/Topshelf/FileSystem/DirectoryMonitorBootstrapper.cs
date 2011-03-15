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
namespace Topshelf.FileSystem
{
	using System;
	using System.Configuration;
	using System.IO;
	using Configuration.Dsl;
	using Internal;
	using Shelving;


	public class DirectoryMonitorBootstrapper :
		Bootstrapper<DirectoryMonitor>
	{
		public void InitializeHostedService([NotNull] IServiceConfigurator<DirectoryMonitor> cfg)
		{
			if (cfg == null)
				throw new ArgumentNullException("cfg");

			cfg.ConstructUsing((d, name, channel) => new DirectoryMonitor(GetServicesDirectory(), channel));

			cfg.WhenStarted(dm => dm.Start());
			cfg.WhenStopped(dm => dm.Stop());
		}

		static string GetServicesDirectory()
		{
			return ConfigurationManager.AppSettings["MonitorDirectory"] ?? GetDefaultServicesDirectory();
		}

		static string GetDefaultServicesDirectory()
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services");
		}
	}
}