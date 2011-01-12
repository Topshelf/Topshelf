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
namespace Topshelf.Configuration.Dsl
{
	using System;
	using System.Collections.Generic;
	using System.ServiceProcess;
	using Magnum.Extensions;
	using Model;
	using Stact;


	public class RunnerConfigurator :
		IRunnerConfigurator
	{
		readonly IList<Action<IServiceCoordinator>> _serviceConfigurators;
		readonly WinServiceSettings _winServiceSettings;
		Action<IServiceCoordinator> _afterStartingServices = c => { };
		Action<IServiceCoordinator> _afterStoppingServices = c => { };
		Action<IServiceCoordinator> _beforeStartingServices = c => { };
		Credentials _credentials;
		bool _disposed;
		TimeSpan _timeout = 1.Minutes();

		/// <summary>
		///   Initializes a new instance of the <see cref = "RunnerConfigurator" /> class.
		/// </summary>
		RunnerConfigurator()
		{
			_winServiceSettings = new WinServiceSettings();
			_winServiceSettings.ServiceName = new ServiceName("Topshelf Service");
			_credentials = Credentials.LocalSystem;
			_serviceConfigurators = new List<Action<IServiceCoordinator>>();
		}

		public void SetDisplayName(string displayName)
		{
			_winServiceSettings.DisplayName = displayName;
		}

		public void SetServiceName(string serviceName)
		{
			_winServiceSettings.ServiceName = new ServiceName(serviceName);
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
			DependsOn(KnownServiceNames.Msmq);
		}

		public void DependencyOnMsSql()
		{
			DependsOn(KnownServiceNames.SqlServer);
		}

		public void DependencyOnEventLog()
		{
			DependsOn(KnownServiceNames.EventLog);
		}

		public void DependencyOnIis()
		{
			DependsOn(KnownServiceNames.IIS);
		}

		public void ConfigureService<TService>(Action<IServiceConfigurator<TService>> action)
			where TService : class
		{
			var configurator = new ServiceConfigurator<TService>();
			_serviceConfigurators.Add(coordinator =>
				{
					action(configurator);

					coordinator.CreateService(configurator.Name, configurator.Create);
				});
		}

		public void BeforeStartingServices(Action<IServiceCoordinator> action)
		{
			_beforeStartingServices = action;
		}

		public void AfterStartingServices(Action<IServiceCoordinator> action)
		{
			_afterStartingServices = action;
		}

		public void AfterStoppingServices(Action<IServiceCoordinator> action)
		{
			_afterStoppingServices = action;
		}

		public void SetEventTimeout(TimeSpan timeout)
		{
			_timeout = timeout;
		}

		/// <summary>
		///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///   Configures a service using the default configuration.
		/// </summary>
		/// <typeparam name = "TService">The type of the service that will be configured.</typeparam>
		public void ConfigureService<TService>()
			where TService : class
		{
			ConfigureService<TService>(x => { });
		}

		/// <summary>
		///   Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name = "disposing"><see langword = "true" /> to release both managed and unmanaged resources; 
		///   <see langword = "false" /> to release only unmanaged resources.</param>
		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
				_serviceConfigurators.Clear();
			_disposed = true;
		}

		RunConfiguration Create()
		{
			var serviceCoordinator = new ServiceCoordinator(new PoolFiber(),
			                                                _beforeStartingServices,
			                                                _afterStartingServices,
			                                                _afterStoppingServices,
			                                                _timeout);

			RegisterServices(serviceCoordinator);

			_winServiceSettings.Credentials = _credentials;

			var cfg = new RunConfiguration
				{
					WinServiceSettings = _winServiceSettings,
					Coordinator = serviceCoordinator
				};

			return cfg;
		}

		void RegisterServices(IServiceCoordinator serviceCoordinator)
		{
			_serviceConfigurators.Each(configurator => configurator(serviceCoordinator));
		}

		/// <summary>
		///   Releases unmanaged resources and performs other cleanup operations before the
		///   <see cref = "RunnerConfigurator" /> is reclaimed by garbage collection.
		/// </summary>
		~RunnerConfigurator()
		{
			Dispose(false);
		}

		public static RunConfiguration New(Action<IRunnerConfigurator> action)
		{
			using (var configurator = new RunnerConfigurator())
			{
				action(configurator);
				return configurator.Create();
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

		public void RunAsNetworkService()
		{
			_credentials = new Credentials(string.Empty, string.Empty, ServiceAccount.NetworkService);
		}

		public void RunAsFromCommandLine()
		{
            //this doesn't really do anything
			//_credentials should be set from the command line
		}

		public void RunAs(string username, string password)
		{
			_credentials = Credentials.Custom(username, password);
		}

		#endregion
	}
}