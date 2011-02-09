// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using Configuration;
	using Extensions;
	using HostConfigurators;
	using Hosts;
	using log4net;
	using Magnum.Extensions;
	using Model;
	using Stact;


	public class RunBuilder :
		HostBuilder
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Builders.RunBuilder");

		readonly IList<Action> _postStartActions = new List<Action>();
		readonly IList<Action> _postStopActions = new List<Action>();
		readonly IList<Action> _preStartActions = new List<Action>();
		readonly IList<Action> _preStopActions = new List<Action>();
		readonly IList<ServiceBuilder> _serviceBuilders = new List<ServiceBuilder>();
		readonly string _serviceName;
		IServiceCoordinator _coordinator;
		TimeSpan _timeout = 1.Minutes();


		public RunBuilder(HostConfiguration configuration)
		{
			_serviceName = configuration.ServiceName;
		}

		public Host Build()
		{
			_coordinator = new ServiceCoordinator(new PoolFiber(),
			                                      ExecutePreStartActions,
			                                      ExecutePostStartActions,
			                                      ExecutePostStopActions,
			                                      _timeout);

			_serviceBuilders.Each(x =>
				{
					_coordinator.CreateService(x.Name, x.Build);
				});

			return CreateHost(new ServiceName(_serviceName), _coordinator);
		}

		public void Match<T>(Action<T> callback)
			where T : class, HostBuilder
		{
			if (this is T)
				callback(this as T);
		}

		Host CreateHost(ServiceName serviceName, IServiceCoordinator coordinator)
		{
			if (Process.GetCurrentProcess().GetParent().ProcessName == "services")
			{
				_log.Debug("Running as a Windows service, using the service host");

				return new WinServiceHost(serviceName, coordinator);
			}

			return new ConsoleRunHost(serviceName, coordinator);
		}

		public void AddServiceBuilder(ServiceBuilder serviceBuilder)
		{
			_serviceBuilders.Add(serviceBuilder);
		}

		public void BeforeStartingServices(Action callback)
		{
			_preStartActions.Add(callback);
		}

		public void AfterStartingServices(Action callback)
		{
			_postStartActions.Add(callback);
		}

		public void BeforeStoppingServices(Action callback)
		{
			_preStopActions.Add(callback);
		}

		public void AfterStoppingServices(Action callback)
		{
			_postStopActions.Add(callback);
		}


		void ExecutePreStartActions(IServiceCoordinator coordinator)
		{
			_preStartActions.Each(x => x());
		}

		void ExecutePostStartActions(IServiceCoordinator coordinator)
		{
			_postStartActions.Each(x => x());
		}

		void ExecutePreStopActions(IServiceCoordinator coordinator)
		{
			_preStopActions.Each(x => x());
		}

		void ExecutePostStopActions(IServiceCoordinator coordinator)
		{
			_postStopActions.Each(x => x());
		}
	}
}