﻿// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Host
{
	using System;
	using System.IO;
	using Dashboard;
	using log4net;
	using log4net.Config;


	public class Program
	{
		static readonly ILog _log = LogManager.GetLogger(Host.DefaultServiceName);

		[LoaderOptimization(LoaderOptimization.MultiDomainHost)]
		static void Main()
		{
			BootstrapLogger();

			HostFactory.Run(x =>
				{
					x.BeforeStartingServices(() => { Console.WriteLine("[Topshelf] Preparing to start host services"); });

					x.AfterStartingServices(() => { Console.WriteLine("[Topshelf] All services have been started"); });

					x.SetServiceName(Host.DefaultServiceName);
					x.SetDisplayName(Host.DefaultServiceName);
					x.SetDescription("Topshelf Service Host");

					x.RunAsLocalSystem();

					x.EnableDashboard();

					x.Service<Host>(y =>
						{
							y.SetServiceName(Host.DefaultServiceName);
							y.ConstructUsing((name, coordinator) => new Host(coordinator));
							y.WhenStarted(host => host.Start());
							y.WhenStopped(host => host.Stop());
						});

					x.AfterStoppingServices(() => { Console.WriteLine("[Topshelf] All services have been stopped"); });
				});
		}

		static void BootstrapLogger()
		{
			string configurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

			var configurationFile = new FileInfo(configurationFilePath);

			XmlConfigurator.ConfigureAndWatch(configurationFile);

			_log.DebugFormat("Logging configuration loaded: {0}", configurationFilePath);
		}
	}
}