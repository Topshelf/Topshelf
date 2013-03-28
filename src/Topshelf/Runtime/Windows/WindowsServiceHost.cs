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
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;
#if !NET35
    using System.Threading.Tasks;
#endif
    using Logging;

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
        bool _disposed;

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
            CanShutdown = settings.CanShutdown;
            CanHandleSessionChangeEvent = settings.CanHandleSessionChangeEvent;
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _serviceHandle.SessionEvent(this, changeDescription.Reason, changeDescription.SessionId);

            base.OnSessionChange(changeDescription);
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            ExitCode = (int)TopshelfExitCode.Ok;

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

            return (TopshelfExitCode)Enum.ToObject(typeof(TopshelfExitCode), ExitCode);
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

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                _log.DebugFormat("[Topshelf] Current Directory: {0}", Directory.GetCurrentDirectory());

                _log.DebugFormat("[Topshelf] Arguments: {0}", string.Join(",", args));

                if (!_serviceHandle.Start(this))
                {
                    throw new TopshelfException("The service did not start successfully (returned false).");
                }

                _log.Info("[Topshelf] Started");
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not start successfully", ex);
                _log.Fatal(ex);

                ExitCode = (int)TopshelfExitCode.StartServiceFailed;
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _log.Info("[Topshelf] Stopping");

                if(!_serviceHandle.Stop(this))
                    throw new TopshelfException("The service did not stop successfully (return false).");

                _log.Info("[Topshelf] Stopped");
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                ExitCode = (int)TopshelfExitCode.StopServiceFailed;
                throw;
            }
        }

        protected override void OnPause()
        {
            try
            {
                _log.Info("[Topshelf] Pausing service");

                if(!_serviceHandle.Pause(this))
                    throw new TopshelfException("The service did not pause successfully (returned false).");

                _log.Info("[Topshelf] Paused");
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not pause gracefully", ex);
                throw;
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _log.Info("[Topshelf] Resuming service");

                if(!_serviceHandle.Continue(this))
                    throw new TopshelfException("The service did not continue successfully (returned false).");

                _log.Info("[Topshelf] Resumed");
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not resume successfully", ex);
                throw;
            }
        }

        protected override void OnShutdown()
        {
            try
            {
                _log.Info("[Topshelf] Service is being shutdown");

                _serviceHandle.Shutdown(this);

                _log.Info("[Topshelf] Stopped");
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                ExitCode = (int)TopshelfExitCode.StopServiceFailed;
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_serviceHandle != null)
                    _serviceHandle.Dispose();

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            ExitCode = (int)TopshelfExitCode.UnhandledServiceException;

            Stop();


#if !NET35
            // it isn't likely that a TPL thread should land here, but if it does let's no block it
            if (Task.CurrentId.HasValue)
            {
                return;
            }
#endif

            int deadThreadId = Interlocked.Increment(ref _deadThread);
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId.ToString();
            while (true)
                Thread.Sleep(TimeSpan.FromHours(1));
        }
    }
}