// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using System.Windows.Forms;
    using Internal;
    using Internal.Actions;

    public class RunnerConfigurator :
        IRunnerConfigurator
    {
        private bool _disposed;
        private readonly WinServiceSettings _winServiceSettings;
        private Credentials _credentials;
        private readonly IList<IService> _services;
        private NamedAction _runnerAction;
        private Type _winForm;
        private Action<IServiceCoordinator> _beforeStart;
        private Action<IServiceCoordinator> _afterStop;


        private RunnerConfigurator()
        {
            _winServiceSettings = new WinServiceSettings();
            _credentials = Credentials.LocalSystem;
            _services = new List<IService>();
            _runnerAction = NamedAction.Console;
            _winForm = typeof(ServiceConsole);
        }

        #region WinServiceSettings
        public void SetDisplayName(string displayName)
        {
            _winServiceSettings.DisplayName = displayName;
        }

        public void SetServiceName(string serviceName)
        {
            _winServiceSettings.ServiceName = serviceName;
        }

        public void SetDescription(string description)
        {
            _winServiceSettings.Description = description;
        }
        public void DoNotStartAutomatically()
        {
            _winServiceSettings.StartMode = ServiceStartMode.Manual;
        }


        public void DependsOn(string serviceName)
        {
            _winServiceSettings.Dependencies.Add(serviceName);
        }

        public void DependencyOnMsmq()
        {
            DependsOn("MSMQ");
        }

        public void DependencyOnMsSql()
        {
            DependsOn("MSSQLSERVER");
        }
        #endregion


        //winservice configuration
        public void ConfigureService<TService>()
        {
            using (var configurator = new ServiceConfigurator<TService>())
            {
                _services.Add(configurator.Create());
            }
        }

        //winservice configuration
        public void ConfigureService<TService>(Action<IServiceConfigurator<TService>> action)
        {
            using(var configurator = new ServiceConfigurator<TService>())
            {
                action(configurator);
                _services.Add(configurator.Create());
            }
        }

        #region Credentials
        public void RunAsLocalSystem()
        {
            _credentials = Credentials.LocalSystem;
        }

        public void RunAsFromInteractive()
        {
            _credentials = Credentials.Interactive;
        }

        public void RunAs(string username, string password)
        {
            _credentials = Credentials.Custom(username, password);
        }
        #endregion

        public void UseWinFormHost<T>() where T : Form
        {
            _runnerAction = NamedAction.Gui;
            _winForm = typeof (T);
        }


        public static IRunConfiguration New(Action<IRunnerConfigurator> action)
        {

            using (var configurator = new RunnerConfigurator())
            {
                action(configurator);

                return configurator.Create();
            }
        }

        public IRunConfiguration Create()
        {
            ServiceCoordinator serviceCoordinator = new ServiceCoordinator(_beforeStart, _afterStop);
            serviceCoordinator.RegisterServices(_services);

            RunConfiguration cfg = new RunConfiguration()
            {
                WinServiceSettings = _winServiceSettings,
                Credentials = _credentials,
                Coordinator = serviceCoordinator
            };
            cfg.SetRunnerAction(_runnerAction, _winForm);

            return cfg;
        }

        public void BeforeStart(Action<IServiceCoordinator> action)
        {
            _beforeStart = action;
        }

        public void AfterStop(Action<IServiceCoordinator> action)
        {
            _afterStop = action;
        }

        #region Dispose Crap
        ~RunnerConfigurator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {

            }
            _disposed = true;
        }
        #endregion
    }
}