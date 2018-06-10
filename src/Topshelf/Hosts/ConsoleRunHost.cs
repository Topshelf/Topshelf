﻿// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Logging;
#if !NETCORE
    using Microsoft.Win32;
#endif
    using Runtime;


    public class ConsoleRunHost :
        Host,
        HostControl
    {
        readonly LogWriter _log = HostLogger.Get<ConsoleRunHost>();
        readonly HostEnvironment _environment;
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        int _deadThread;

        TopshelfExitCode _exitCode;
        ManualResetEvent _exit;
        volatile bool _hasCancelled;

        public ConsoleRunHost(HostSettings settings, HostEnvironment environment, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            _settings = settings;
            _environment = environment;
            _serviceHandle = serviceHandle;

#if !NETCORE
            if (settings.CanSessionChanged)
            {
                SystemEvents.SessionSwitch += OnSessionChanged;
            }

            if (settings.CanHandlePowerEvent)
            {
                SystemEvents.PowerModeChanged += OnPowerModeChanged;
            }
    #endif
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            if (_environment.IsServiceInstalled(_settings.ServiceName))
            {
                if (!_environment.IsServiceStopped(_settings.ServiceName))
                {
                    _log.ErrorFormat("The {0} service is running and must be stopped before running via the console",
                        _settings.ServiceName);

                    return TopshelfExitCode.ServiceAlreadyRunning;
                }
            }

            bool started = false;
            try
            {
                _log.Debug("Starting up as a console application");

                _exit = new ManualResetEvent(false);
                _exitCode = TopshelfExitCode.Ok;

                Console.Title = _settings.DisplayName;
                Console.CancelKeyPress += HandleCancelKeyPress;

                if (!_serviceHandle.Start(this))
                    throw new TopshelfException("The service failed to start (return false).");

                started = true;

                _log.InfoFormat("The {0} service is now running, press Control+C to exit.", _settings.ServiceName);

                _exit.WaitOne();
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("An exception occurred", ex);

                return TopshelfExitCode.AbnormalExit;
            }
            finally
            {
                if (started)
                    StopService();

                _exit.Close();
                (_exit as IDisposable).Dispose();

                HostLogger.Shutdown();
            }

            return _exitCode;
        }


        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // good for you, maybe we'll use a timer for startup at some point but for debugging
            // it's a pain in the ass
        }


        void HostControl.Stop()
        {
            _log.Info("Service Stop requested, exiting.");
            _exit.Set();
        }

        void HostControl.Stop(TopshelfExitCode exitCode)
        {
            _log.Info($"Service Stop requested with exit code {exitCode}, exiting.");
            _exitCode = exitCode;
            _exit.Set();
        }

        void HostControl.Restart()
        {
            _log.Info("Service Restart requested, but we don't support that here, so we are exiting.");
            _exit.Set();
        }


        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _settings.ExceptionCallback?.Invoke((Exception) e.ExceptionObject);

            _log.Fatal("The service threw an unhandled exception", (Exception) e.ExceptionObject);

            HostLogger.Shutdown();

            if (e.IsTerminating)
            {
                _exitCode = TopshelfExitCode.UnhandledServiceException;
                _exit.Set();

                // it isn't likely that a TPL thread should land here, but if it does let's no block it
                if (Task.CurrentId.HasValue)
                {
                    return;
                }

                // this is evil, but perhaps a good thing to let us clean up properly.
                int deadThreadId = Interlocked.Increment(ref _deadThread);
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId.ToString();
                while (true)
                    Thread.Sleep(TimeSpan.FromHours(1));
            }
        }


        void StopService()
        {
            try
            {
                if (_hasCancelled == false)
                {
                    _log.InfoFormat("Stopping the {0} service", _settings.ServiceName);

                    if (!_serviceHandle.Stop(this))
                        throw new TopshelfException("The service failed to stop (returned false).");
                }
            }
            catch (Exception ex)
            {
                _settings.ExceptionCallback?.Invoke(ex);

                _log.Error("The service did not shut down gracefully", ex);
                _exitCode = TopshelfExitCode.StopServiceFailed;
            }
            finally
            {
                _serviceHandle.Dispose();

                _log.InfoFormat("The {0} service has stopped.", _settings.ServiceName);
            }
        }


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

            _log.Info("Control+C detected, attempting to stop service.");
            if (_serviceHandle.Stop(this))
            {
                _hasCancelled = true;
                _exit.Set();
            }
            else
            {
                _hasCancelled = false;
                _log.Error("The service is not in a state where it can be stopped.");
            }
        }

#if !NETCORE
        void OnSessionChanged(object sender, SessionSwitchEventArgs e)
        {
            var arguments = new ConsoleSessionChangedArguments(e.Reason);

            _serviceHandle.SessionChanged(this, arguments);
        }

        void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            var arguments = new ConsolePowerEventArguments(e.Mode);

            _serviceHandle.PowerEvent(this, arguments);
        }


        class ConsoleSessionChangedArguments :
            SessionChangedArguments
        {
            public ConsoleSessionChangedArguments(SessionSwitchReason reason)
            {
                ReasonCode = (SessionChangeReasonCode) Enum.ToObject(typeof(SessionChangeReasonCode), (int) reason);
                SessionId = Process.GetCurrentProcess().SessionId;
            }

            public SessionChangeReasonCode ReasonCode { get; }

            public int SessionId { get; }
        }

        class ConsolePowerEventArguments :
            PowerEventArguments
        {
            public ConsolePowerEventArguments(PowerModes powerMode)
            {
                switch (powerMode)
                {
                    case PowerModes.Resume:
                        EventCode = PowerEventCode.ResumeAutomatic;
                        break;
                    case PowerModes.StatusChange:
                        EventCode = PowerEventCode.PowerStatusChange;
                        break;
                    case PowerModes.Suspend:
                        EventCode = PowerEventCode.Suspend;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(powerMode), powerMode, null);
                }
            }

            public PowerEventCode EventCode { get; }
        }
#endif
    }
}