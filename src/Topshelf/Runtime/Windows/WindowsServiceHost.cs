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
    using Logging;
    using Runtime;

    public class WindowsServiceHost :
        ServiceBase,
        Host,
        HostControl
    {
        static readonly Log _log = Logger.Get<WindowsServiceHost>();
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        HostEnvironment _environment;
        HostControl _hostControl;

        public WindowsServiceHost(HostEnvironment environment, HostSettings settings, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (serviceHandle == null)
                throw new ArgumentNullException("serviceHandle");

            _settings = settings;
            _serviceHandle = serviceHandle;
            _environment = environment;
        }

        public void Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _log.Info("Starting as a Windows service");

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                string message = string.Format("The {0} service has not been installed yet. Please run '{1} install'.",
                    _settings, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);

                throw new TopshelfException(message);
            }

            _log.Debug("[Topshelf] Starting up as a windows service application");

            Run(this);
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            RequestAdditionalTime((int)timeRemaining.TotalMilliseconds);
        }

        void HostControl.Restart()
        {
            throw new NotImplementedException("This is not done yet, so I'm trying");
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("[Topshelf] Starting");

                _log.DebugFormat("[Topshelf] Arguments: {0}", string.Join(",", args));

                _serviceHandle.Start(_hostControl);
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

                _serviceHandle.Stop(_hostControl);
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

                _serviceHandle.Pause(_hostControl);
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

                _serviceHandle.Continue(_hostControl);
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
    }
}