// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading;
    using Logging;
    using Runtime;

    public class WindowsServiceHost :
        ServiceBase,
        Host,
        HostControl
    {
        static readonly LogWriter _log = HostLogger.Get<WindowsServiceHost>();
        readonly HostEnvironment _environment;
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        int _deadThread;

        public WindowsServiceHost(HostEnvironment environment, HostSettings settings, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (serviceHandle == null)
                throw new ArgumentNullException("serviceHandle");

            _settings = settings;
            _serviceHandle = serviceHandle;
            _environment = environment;

            CanPauseAndContinue = settings.CanPauseAndContinue;
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            _log.Info("Starting as a Windows service");

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                string message = string.Format("The {0} service has not been installed yet. Please run '{1} install'.",
                    _settings, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);

                ExitCode = (int)TopshelfExitCode.ServiceNotInstalled;
                throw new TopshelfException(message);
            }

            _log.Debug("[Topshelf] Starting up as a windows service application");

            Run(this);

            ExitCode = (int)TopshelfExitCode.Ok;
            return TopshelfExitCode.Ok;
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            _log.DebugFormat("Requesting additional time: {0}", timeRemaining);

            RequestAdditionalTime((int)timeRemaining.TotalMilliseconds);
        }

        void HostControl.Restart()
        {
            _log.Fatal("Restart is not yet implemented");

            throw new NotImplementedException("This is not done yet, so I'm trying");
        }

        void HostControl.Stop()
        {
            if (CanStop)
            {
                _log.Debug("Stop requested by hosted service");
                Stop();
            }
            else
            {
                _log.Debug("Stop requested by hosted service, but service cannot be stopped at this time");
                throw new ServiceControlException("The service cannot be stopped at this time");
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("[Topshelf] Starting");

                _log.DebugFormat("[Topshelf] Arguments: {0}", string.Join(",", args));

                _serviceHandle.Start(this);
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

                _serviceHandle.Stop(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Stopped");
            }
        }

        protected override void OnPause()
        {
            try
            {
                _log.Info("[Topshelf] Pausing service");

                _serviceHandle.Pause(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Paused");
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _log.Info("[Topshelf] Pausing service");

                _serviceHandle.Continue(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Paused");
            }
        }

        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            Stop();

            int deadThreadId = Interlocked.Increment(ref _deadThread);
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId.ToString();
            while (true)
                Thread.Sleep(TimeSpan.FromHours(1));
        }
    }
}