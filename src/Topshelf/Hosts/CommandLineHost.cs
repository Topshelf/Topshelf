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
namespace Topshelf.Hosts
{
	using System;
	using System.Linq;
	using System.ServiceProcess;
	using System.Threading;
	using Configuration;
	using log4net;
	using Model;


	public class CommandLineHost :
		Host
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(CommandLineHost));
		readonly ServiceName _serviceName;
		IServiceCoordinator _coordinator;
		ManualResetEvent _exit;

		public CommandLineHost(ServiceName name, IServiceCoordinator coordinator)
		{
			_serviceName = name;
			_coordinator = coordinator;
		}

		public void Host()
		{
			CheckToSeeIfWinServiceRunning();

			try
			{
				_log.Debug("Starting up as a console application");

				_exit = new ManualResetEvent(false);

				Console.CancelKeyPress += HandleCancelKeyPress;

				_coordinator.Start(); //user code starts

				_log.InfoFormat("[Topshelf] Running, press Control+C to exit.");

				_exit.WaitOne();
			}
			catch (Exception ex)
			{
				_log.Error("An exception occurred", ex);
			}
			finally
			{
				ShutdownCoordinator();
			}
			_exit.Close();
			_exit = null;
		}

		void ShutdownCoordinator()
		{
			try
			{
				_log.Info("[Topshelf] Stopping");

				_coordinator.Stop();
			}
			catch (Exception ex)
			{
				_log.Error("The service did not shut down gracefully", ex);
			}
			finally
			{
				_coordinator.Dispose();
				_coordinator = null;

				_log.Info("[Topshelf] Stopped");
			}
		}

		void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
		{
			if (consoleCancelEventArgs.SpecialKey == ConsoleSpecialKey.ControlBreak)
			{
				_log.Error("Control+Break detected, terminating service (not cleanly, use Control+C to exit cleanly)");
				return;
			}

			_log.Info("Control+C detected, exiting.");
			_exit.Set();

			consoleCancelEventArgs.Cancel = true;
		}

		void CheckToSeeIfWinServiceRunning()
		{
			if (ServiceController.GetServices().Where(s => s.ServiceName == _serviceName.FullName).Any())
				_log.WarnFormat("There is an instance of this {0} running as a windows service", _serviceName);
		}
	}
}