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
namespace Topshelf.Hosts
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.ServiceProcess;
	using System.Threading;
	using Internal;
	using log4net;
	using Model;


	public class ConsoleRunHost :
		Host, IDisposable
	{
		readonly ServiceDescription _description;
		readonly ILog _log = LogManager.GetLogger("Topshelf.Hosts.ConsoleRunHost");
		IServiceCoordinator _coordinator;
		ManualResetEvent _exit;
		volatile bool _hasCancelled;

		public ConsoleRunHost([NotNull] ServiceDescription description, [NotNull] IServiceCoordinator coordinator)
		{
			if (description == null)
				throw new ArgumentNullException("description");
			if (coordinator == null)
				throw new ArgumentNullException("coordinator");

			_description = description;
			_coordinator = coordinator;
		}

		public void Run()
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

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
				_exit.Close();
			}
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

		[MethodImpl(MethodImplOptions.Synchronized)]
		void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
		{
			if (consoleCancelEventArgs.SpecialKey == ConsoleSpecialKey.ControlBreak)
			{
				_log.Error("Control+Break detected, terminating service (not cleanly, use Control+C to exit cleanly)");
				return;
			}

			consoleCancelEventArgs.Cancel = true;

			if (_hasCancelled)
				return;

			_log.Info("Control+C detected, exiting.");
			_exit.Set();

			_hasCancelled = true;
		}

		void CheckToSeeIfWinServiceRunning()
		{
			if (ServiceController.GetServices().Where(s => s.ServiceName == _description.GetServiceName()).Any())
				_log.WarnFormat("There is an instance of this {0} running as a windows service", _description);
		}

		bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				(_exit as IDisposable).Dispose();
			}

			_disposed = true;
		}
	}
}