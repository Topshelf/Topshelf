// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Threading.Tasks;
    using HostConfigurators;
    using Logging;

    public class WindowsServiceHost :
        ServiceBase,
        Host,
        HostControl
    {
        static readonly LogWriter _log = HostLogger.Get<WindowsServiceHost>();
        readonly HostConfigurator _configurator;
        readonly HostEnvironment _environment;
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        int _deadThread;
        bool _disposed;
        Exception _unhandledException;

        public WindowsServiceHost(HostEnvironment environment, HostSettings settings, ServiceHandle serviceHandle, HostConfigurator configurator)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (serviceHandle == null)
                throw new ArgumentNullException(nameof(serviceHandle));

            _settings = settings;
            _serviceHandle = serviceHandle;
            _environment = environment;
            _configurator = configurator;

            CanPauseAndContinue = settings.CanPauseAndContinue;
            CanShutdown = settings.CanShutdown;
            CanHandlePowerEvent = settings.CanHandlePowerEvent;
            CanHandleSessionChangeEvent = settings.CanSessionChanged;
            ServiceName = _settings.ServiceName;
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            ExitCode = (int) TopshelfExitCode.Ok;

            _log.Info("Starting as a Windows service");

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                string message = $"The {_settings.ServiceName} service has not been installed yet. Please run '{Assembly.GetEntryAssembly().GetName()} install'.";
                _log.Fatal(message);

                ExitCode = (int) TopshelfExitCode.ServiceNotInstalled;
                throw new TopshelfException(message);
            }

            _log.Debug("[Topshelf] Starting up as a windows service application");

            Run(this);

            return (TopshelfExitCode) Enum.ToObject(typeof(TopshelfExitCode), ExitCode);
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            _log.DebugFormat("Requesting additional time: {0}", timeRemaining);

            RequestAdditionalTime((int) timeRemaining.TotalMilliseconds);
        }

        void HostControl.Stop()
        {
            InternalStop();
        }

        void HostControl.Stop(TopshelfExitCode exitCode)
        {
            InternalStop(exitCode);
        }

        void InternalStop(TopshelfExitCode? exitCode = null)

        {
            if (CanStop)
            {
                _log.Debug("Stop requested by hosted service");
                if (exitCode.HasValue)
                    ExitCode = (int) exitCode.Value;
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

                string startArgs = string.Join(" ", args);
                _configurator.ApplyCommandLine(startArgs);

                if (!_serviceHandle.Start(this))
                    throw new TopshelfException("The service did not start successfully (returned false).");

                _log.Info("[Topshelf] Started");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Fatal("The service did not start successfully", ex);

                ExitCode = (int) TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _log.Info("[Topshelf] Stopping");

                if (!_serviceHandle.Stop(this))
                    throw new TopshelfException("The service did not stop successfully (return false).");

                _log.Info("[Topshelf] Stopped");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Fatal("The service did not shut down gracefully", ex);
                ExitCode = (int) TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }

            if (_unhandledException != null)
            {
                ExitCode = (int) TopshelfExitCode.AbnormalExit;
                _log.Info("[Topshelf] Unhandled exception detected, rethrowing to cause application to restart.");
                throw new InvalidOperationException("An unhandled exception was detected", _unhandledException);
            }
        }

        protected override void OnPause()
        {
            try
            {
                _log.Info("[Topshelf] Pausing service");

                if (!_serviceHandle.Pause(this))
                    throw new TopshelfException("The service did not pause successfully (returned false).");

                _log.Info("[Topshelf] Paused");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Fatal("The service did not pause gracefully", ex);
                throw;
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _log.Info("[Topshelf] Resuming service");

                if (!_serviceHandle.Continue(this))
                    throw new TopshelfException("The service did not continue successfully (returned false).");

                _log.Info("[Topshelf] Resumed");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

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
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Fatal("The service did not shut down gracefully", ex);
                ExitCode = (int) TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            try
            {
                _log.Info("[Topshelf] Service session changed");

                var arguments = new WindowsSessionChangedArguments(changeDescription);

                _serviceHandle.SessionChanged(this, arguments);

                _log.Info("[Topshelf] Service session changed handled");
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Fatal("The did not handle Service session change correctly", ex);
                ExitCode = (int) TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            try
            {
                _log.Info("[Topshelf] Power event raised");

                var arguments = new WindowsPowerEventArguments(powerStatus);

                bool result = _serviceHandle.PowerEvent(this, arguments);

                _log.Info("[Topshelf] Power event handled");

                return result;
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Fatal("The service did handle the Power event correctly", ex);
                ExitCode = (int) TopshelfExitCode.ServiceControlRequestFailed;
                throw;
            }
        }

        protected override void OnCustomCommand(int command)
        {
            try
            {
                _log.InfoFormat("[Topshelf] Custom command {0} received", command);

                _serviceHandle.CustomCommand(this, command);

                _log.InfoFormat("[Topshelf] Custom command {0} processed", command);
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("Unhandled exception during custom command processing detected", ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _serviceHandle?.Dispose();

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _settings.ExceptionCallback?.Invoke((Exception) e.ExceptionObject);

            if (_settings.UnhandledExceptionPolicy == UnhandledExceptionPolicyCode.TakeNoAction)
              return;

            _log.Fatal("The service threw an unhandled exception", (Exception) e.ExceptionObject);

            if (_settings.UnhandledExceptionPolicy == UnhandledExceptionPolicyCode.LogErrorOnly)
              return;

            HostLogger.Shutdown();

//            // IF not terminating, then no reason to stop the service?
//            if (!e.IsTerminating)
//                return;
//          This needs to be a configuration option to avoid breaking compatibility

            ExitCode = (int) TopshelfExitCode.AbnormalExit;
            _unhandledException = (Exception) e.ExceptionObject;

            Stop();


            // it isn't likely that a TPL thread should land here, but if it does let's no block it
            if (Task.CurrentId.HasValue)
                return;

            int deadThreadId = Interlocked.Increment(ref _deadThread);
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId;
            while (true)
                Thread.Sleep(TimeSpan.FromHours(1));
        }


        class WindowsSessionChangedArguments :
            SessionChangedArguments
        {
            public WindowsSessionChangedArguments(SessionChangeDescription changeDescription)
            {
                ReasonCode = (SessionChangeReasonCode) Enum.ToObject(typeof(SessionChangeReasonCode), (int) changeDescription.Reason);
                SessionId = changeDescription.SessionId;
            }

            public SessionChangeReasonCode ReasonCode { get; }

            public int SessionId { get; }
        }

        class WindowsPowerEventArguments :
            PowerEventArguments
        {
            public WindowsPowerEventArguments(PowerBroadcastStatus powerStatus)
            {
                EventCode = (PowerEventCode) Enum.ToObject(typeof(PowerEventCode), (int) powerStatus);
            }


            public PowerEventCode EventCode { get; }
        }
    }
}
