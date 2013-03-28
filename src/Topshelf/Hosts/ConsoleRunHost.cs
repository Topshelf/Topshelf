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
namespace Topshelf.Hosts
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
#if !NET35
    using System.Threading.Tasks;
#endif
    using Logging;
    using Runtime;
    using System.ServiceProcess;
    using Microsoft.Win32;

    public class ConsoleRunHost :
        Host,
        HostControl
    {
        readonly LogWriter _log = HostLogger.Get<ConsoleRunHost>();
        readonly HostEnvironment _environment;
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        int _deadThread;

        ManualResetEvent _exit;
        volatile bool _hasCancelled;

        public ConsoleRunHost(HostSettings settings, HostEnvironment environment, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (environment == null)
                throw new ArgumentNullException("environment");

            _settings = settings;
            _environment = environment;
            _serviceHandle = serviceHandle;

            if (_settings.CanHandleSessionChangeEvent)
            {
                SystemEvents.SessionSwitch += new SessionSwitchEventHandler(OnSessionChange);
            }
        }

        void OnSessionChange(object sender, SessionSwitchEventArgs e)
        {
            int sessionid = Process.GetCurrentProcess().SessionId;

            // We can cast the reason directly, the enums are identical
            _serviceHandle.SessionEvent(this, (SessionChangeReason)e.Reason, sessionid);
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

                Console.CancelKeyPress += HandleCancelKeyPress;

                if(!_serviceHandle.Start(this))
                    throw new TopshelfException("The service failed to start (return false).");

                started = true;

                _log.InfoFormat("The {0} service is now running, press Control+C to exit.", _settings.ServiceName);

                _exit.WaitOne();
            }
            catch (Exception ex)
            {
                _log.Error("An exception occurred", ex);

                return TopshelfExitCode.AbnormalExit;
            }
            finally
            {
                if(started)
                    StopService();

                _exit.Close();
                (_exit as IDisposable).Dispose();

                HostLogger.Shutdown();
            }

            return TopshelfExitCode.Ok;
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

        void HostControl.Restart()
        {
            _log.Info("Service Restart requested, but we don't support that here, so we are exiting.");
            _exit.Set();
        }

        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            if (e.IsTerminating)
            {
                _exit.Set();
                
#if !NET35
                // it isn't likely that a TPL thread should land here, but if it does let's no block it
                if(Task.CurrentId.HasValue)
                {
                    return;
                }
#endif

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

                    if(!_serviceHandle.Stop(this))
                        throw new TopshelfException("The service failed to stop (returned false).");
                }
            }
            catch (Exception ex)
            {
                _log.Error("The service did not shut down gracefully", ex);
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
                _exit.Set();
                _hasCancelled = true;
            }
            else
            {
                _hasCancelled = false;
                _log.Error("The service is not in a state where it can be stopped.");
            }
        }
    }
}