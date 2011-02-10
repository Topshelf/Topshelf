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
namespace Topshelf.Windows
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.ServiceProcess;
	using Exceptions;
	using log4net;
	using Model;


	public class WindowsServiceHost :
		ServiceBase,
		Host
	{
		readonly ServiceDescription _description;
		readonly ILog _log = LogManager.GetLogger("Topshelf.Windows.WindowsServiceHost");
		IServiceCoordinator _coordinator;

		public WindowsServiceHost(ServiceDescription description, IServiceCoordinator coordinator)
		{
			_coordinator = coordinator;
			_description = description;
		}

		public void Run()
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			_log.Info("Starting up as a winservice application");

			if (!WindowsServiceControlManager.IsInstalled(_description.GetServiceName()))
			{
				string message =
					string.Format("The {0} service has not been installed yet. Please run '{1} install'.",
					              _description, Assembly.GetEntryAssembly().GetName());
				_log.Fatal(message);
				throw new ConfigurationException(message);
			}

			_log.Debug("[Topshelf] Starting up as a windows service application");

			Run(this);
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				_log.Info("[Topshelf] Starting");

				_log.DebugFormat("[Topshelf] Arguments: {0}", string.Join(",", args));

				_coordinator.Start();
			}
			catch (Exception ex)
			{
				_log.Fatal(ex);
				throw;
			}
		}

		protected override void OnStop()
		{
			try
			{
				_log.Info("[Topshelf] Stopping");

				_coordinator.Stop();
			}
			catch (Exception ex)
			{
				_log.Fatal("The service did not shut down gracefully", ex);
				throw;
			}
			finally
			{
				_coordinator.Dispose();
				_coordinator = null;

				_log.Info("[Topshelf] Stopped");
			}
		}
	}
}