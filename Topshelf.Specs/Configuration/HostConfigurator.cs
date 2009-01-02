namespace Topshelf.Specs.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using System.Windows.Forms;
    using Actions;

    public class HostConfigurator :
        IHostConfigurator
    {
        private bool _disposed;
        private readonly WinServiceSettings _winServiceSettings;
        private Credentials _credentials;
        private readonly IList<IService> _services;
        private NamedAction _runnerAction;
        private Type _winForm;


        private HostConfigurator()
        {
            _winServiceSettings = new WinServiceSettings();
            _credentials = Credentials.LocalSystem;
            _services = new List<IService>();
            _runnerAction = NamedAction.Console;
            _winForm = null;
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

        public void ConfigureService<TService>()
        {
            using (var configurator = new ServiceConfigurator<TService>())
            {
                _services.Add(configurator.Create());
            }
        }

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

        public void UseWinFormRunner<T>() where T : Form
        {
            _runnerAction = NamedAction.Gui;
            _winForm = typeof (T);
        }


        public static IHost New(Action<IHostConfigurator> action)
        {

            using (var configurator = new HostConfigurator())
            {
                action(configurator);

                return configurator.Create();
            }
        }

        public IHost Create()
        {
            Host host = new Host
                            {
                                WinServiceSettings = _winServiceSettings, 
                                Credentials = _credentials
                            };
            host.RegisterServices(_services);
            host.SetRunnerAction(_runnerAction, _winForm);
            return host;
        }

        #region Dispose Crap
        ~HostConfigurator()
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