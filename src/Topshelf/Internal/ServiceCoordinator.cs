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
namespace Topshelf.Internal
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using log4net;

    [DebuggerDisplay("Hosting {HostedServiceCount} Services")]
	public class ServiceCoordinator :
		IServiceCoordinator
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (ServiceCoordinator));
		private readonly Action<IServiceCoordinator> _afterStop;
		private readonly Action<IServiceCoordinator> _beforeStart;
		private readonly Action<IServiceCoordinator> _beforeStartingServices;
		private readonly Dictionary<string, IServiceController> _services = new Dictionary<string, IServiceController>();

		public ServiceCoordinator(Action<IServiceCoordinator> beforeStartingServices, Action<IServiceCoordinator> beforeStart, Action<IServiceCoordinator> afterStop)
		{
			_beforeStartingServices = beforeStartingServices;
			_beforeStart = beforeStart;
			_afterStop = afterStop;
			_serviceConfigurators = new List<Func<IServiceController>>();
		}

		public void Start()
		{
			_log.Debug("Calling BeforeStartingServices");
			_beforeStartingServices(this);
			_log.Info("BeforeStart complete");


			_log.Debug("Start is now starting any subordinate services");

			foreach (Func<IServiceController> serviceConfigurator in _serviceConfigurators)
			{
				IServiceController serviceController = serviceConfigurator();
				_services.Add(serviceController.Name, serviceController);

				_log.InfoFormat("Starting subordinate service '{0}'", serviceController.Name);
				serviceController.Start();
			}

			_log.Debug("Calling BeforeStart");
			_beforeStart(this);
			_log.Info("BeforeStart complete");
		}

		public void Stop()
		{
			foreach (var service in _services.Values.Reverse())
			{
				try
				{
					_log.InfoFormat("Stopping sub service '{0}'", service.Name);
					service.Stop();
				}
				catch (Exception ex)
				{
					_log.Error("Exception stopping sub service " + service.Name, ex);
				}
			}

			_log.Debug("pre after stop");
			_afterStop(this);
			_log.Info("AfterStop complete");
			OnStopped();
		}

		public void Pause()
		{
			foreach (var service in _services.Values)
			{
				_log.InfoFormat("Pausing sub service '{0}'", service.Name);
				service.Pause();
			}
		}

		public void Continue()
		{
			foreach (var service in _services.Values)
			{
				_log.InfoFormat("Continuing sub service '{0}'", service.Name);
				service.Continue();
			}
		}

		public void StartService(string name)
		{
			if (_services.Count == 0)
				CreateServices();

			_services[name].Start();
		}

		public void StopService(string name)
		{
			if (_services.Count == 0)
				CreateServices();

			_services[name].Stop();
		}

		public void PauseService(string name)
		{
			if (_services.Count == 0)
				CreateServices();

			_services[name].Pause();
		}

		public void ContinueService(string name)
		{
			if (_services.Count == 0)
				CreateServices();

			_services[name].Continue();
		}

		public int HostedServiceCount
		{
			get { return _services.Count; }
		}

		public IList<ServiceInformation> GetServiceInfo()
		{
			var result = new List<ServiceInformation>();

			foreach (var value in _services.Values)
			{
				result.Add(new ServiceInformation
					{
						Name = value.Name,
						State = value.State,
						Type = value.ServiceType.Name
					});
			}

			return result;
		}

		public IServiceController GetService(string name)
		{
			return _services[name];
		}

		public event Action Stopped;

		#region Dispose Crap

		private readonly List<Func<IServiceController>> _serviceConfigurators;
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				foreach (var service in _services.Values)
				{
					service.Dispose();
				}
				_services.Clear();
			}
			_disposed = true;
		}

		~ServiceCoordinator()
		{
			Dispose(false);
		}

		#endregion

		public void RegisterServices(IList<Func<IServiceController>> services)
		{
			_serviceConfigurators.AddRange(services);
		}

		private void CreateServices()
		{
			foreach (Func<IServiceController> serviceConfigurator in _serviceConfigurators)
			{
				IServiceController serviceController = serviceConfigurator();
				_services.Add(serviceController.Name, serviceController);
			}
		}

		private void OnStopped()
		{
			Action handler = Stopped;
			if (handler != null)
			{
				handler();
			}
		}
	}
}