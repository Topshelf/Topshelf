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
namespace Topshelf
{
	using System;
	using System.IO;
	using Configuration;
	using Configuration.Dsl;
	using log4net;
	using log4net.Config;


	public class Program
	{
		static readonly ILog _log = LogManager.GetLogger(Host.DefaultServiceName);

		static void Main(string[] args)
		{
			BootstrapLogger();

			RunConfiguration cfg = RunnerConfigurator.New(x =>
				{
					x.AfterStoppingServices(h =>
						{
							Console.WriteLine("[Topshelf] All services have been stopped");
						});

					//x.EnableDashboard();

					x.ConfigureService<Host>(s =>
						{
							s.Named(Host.DefaultServiceName);
							s.ConstructUsing((name,coordinator) => new Host(coordinator));
							s.WhenStarted(tc => tc.Start());
							s.WhenStopped(tc => tc.Stop());
						});

					x.RunAsLocalSystem();

					x.SetDescription("Topshelf Hosting Service");
					x.SetDisplayName(Host.DefaultServiceName);
					x.SetServiceName(Host.DefaultServiceName);
				});

			Runner.Host(cfg, args);
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