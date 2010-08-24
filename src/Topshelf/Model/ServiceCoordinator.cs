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
namespace Topshelf.Model
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using log4net;
	using Magnum;
	using Magnum.Channels;
	using Magnum.Collections;
	using Magnum.Extensions;
	using Magnum.Fibers;
	using Magnum.StateMachine;
	using Messages;
	using Shelving;


	public class ServiceCoordinator :
		IServiceCoordinator
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(ServiceCoordinator));
		readonly Action<IServiceCoordinator> _afterStartingServices;
		readonly Action<IServiceCoordinator> _afterStoppingServices;
		readonly Action<IServiceCoordinator> _beforeStartingServices;
		readonly Fiber _fiber;
		readonly Cache<string, ServiceStateMachine> _serviceCache;
		readonly IDictionary<string, Func<IServiceCoordinator, ServiceStateMachine>> _startupServices;
		readonly AutoResetEvent _updated = new AutoResetEvent(true);
		InboundChannel _channel;

		bool _disposed;

		bool _stopping;

		public ServiceCoordinator(Fiber fiber,
		                          Action<IServiceCoordinator> beforeStartingServices,
		                          Action<IServiceCoordinator> afterStartingServices,
		                          Action<IServiceCoordinator> afterStoppingServices)
		{
			_fiber = fiber;
			_afterStoppingServices = afterStoppingServices;
			_afterStartingServices = afterStartingServices;
			_beforeStartingServices = beforeStartingServices;

			_startupServices = new Dictionary<string, Func<IServiceCoordinator, ServiceStateMachine>>();

			_serviceCache = new Cache<string, ServiceStateMachine>();

			_channel = AddressRegistry.GetServiceCoordinatorHost(x =>
				{
					x.AddConsumersFor<ServiceStateMachine>()
						.BindUsing<ServiceStateMachineBinding, string>()
						.CreateNewInstanceBy(GetServiceInstance)
						.ExecuteOnThreadPoolFiber()
						.PersistInMemoryUsing(_serviceCache);

					x.AddConsumerOf<ServiceEvent>()
						.UsingConsumer(OnServiceEvent)
						.ExecuteOnFiber(_fiber);

					x.AddConsumerOf<ServiceStopped>()
						.UsingConsumer(OnServiceStopped)
						.ExecuteOnFiber(_fiber);
				});

			EventChannel = new ChannelAdapter();
		}

		public ServiceCoordinator()
			: this(new ThreadPoolFiber(), null, null, null)
		{
		}

		public ChannelAdapter EventChannel { get; private set; }


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public int ServiceCount
		{
			get { return _serviceCache.Count(); }
		}

		public IServiceController this[string serviceName]
		{
			get
			{
				if (_serviceCache.Has(serviceName))
					return _serviceCache[serviceName];

				return null;
			}
		}

		public void Start(TimeSpan timeout)
		{
			BeforeStartingServices();

			_startupServices.Each(serviceFactory => _channel.Send(new CreateService(serviceFactory.Key)));

			WaitUntilAllServicesAre(ServiceStateMachine.Running, timeout);

			AfterStartingServices();
		}

		/// <summary>
		///   Creates a shelf service using the specified bootstrapper type
		/// </summary>
		/// <param name = "serviceName">The name of the service to create</param>
		/// <param name = "bootstrapperType">The type of the bootstrapper class for the service</param>
		public void CreateShelfService(string serviceName, Type bootstrapperType)
		{
			_startupServices.Add(serviceName, x => { return new ShelfServiceController(serviceName, _channel); });
			//_channel.Send(new CreateShelfService(serviceName, ShelfType.Internal, bootstrapperType));
		}

		/// <summary>
		///   Creates a shelf service by name, determining the bootstrapper type by reflection
		/// </summary>
		/// <param name = "serviceName">The name of the service to create (should match the folder)</param>
		public void CreateShelfService(string serviceName)
		{
			_startupServices.Add(serviceName, x => { return new ShelfServiceController(serviceName, _channel); });
			//_channel.Send(new CreateShelfService(serviceName, ShelfType.Folder));
		}

		public void CreateService(string serviceName, Func<IServiceCoordinator, ServiceStateMachine> serviceFactory)
		{
			_startupServices.Add(serviceName, serviceFactory);
		}

		public IEnumerator<IServiceController> GetEnumerator()
		{
			return _serviceCache.Cast<IServiceController>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Stop(TimeSpan timeout)
		{
			_stopping = true;

			SendStopCommandToServices();

			WaitUntilAllServicesAre(ServiceStateMachine.Completed, timeout);

			AfterStoppingServices();
		}

		void WaitUntilAllServicesAre(State state, TimeSpan timeout)
		{
			DateTime stopTime = SystemUtil.Now + timeout;

			while (SystemUtil.Now < stopTime)
			{
				_updated.WaitOne(1.Seconds());

				if (AllServiceInState(state))
					break;
			}

			if (!AllServiceInState(state))
				throw new InvalidOperationException("All services were not {0} within the specified timeout".FormatWith(state.Name));
		}

		ServiceStateMachine GetServiceInstance(string key)
		{
			if(key == null)
				return new ServiceStateMachine(null, _channel);

			if (_startupServices.ContainsKey(key))
				return _startupServices[key](this);

			return new ServiceStateMachine(key, _channel);
		}

		~ServiceCoordinator()
		{
			Dispose(false);
		}

		void OnServiceEvent(ServiceEvent message)
		{
			_log.InfoFormat("[{0}] {1}", message.ServiceName, message.EventType);
			_updated.Set();
		}

		void OnServiceStopped(ServiceStopped message)
		{
			if (_stopping)
				_channel.Send(new UnloadService(message.ServiceName));
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_channel != null)
				{
					_log.DebugFormat("[Topshelf] Closing coordinator channel");
					_channel.Dispose();
					_channel = null;
				}
			}

			_disposed = true;
		}

		bool AllServiceInState(State expected)
		{
			return _serviceCache.Count() > 0 && _serviceCache.All(x => x.CurrentState == expected);
		}

		void SendStopCommandToServices()
		{
			_serviceCache.Each((name, service) =>
				{
					var message = new StopService(name);

					_channel.Send(message);
				});
		}


		void BeforeStartingServices()
		{
			CallAction("Before starting services", _beforeStartingServices);
		}

		void AfterStartingServices()
		{
			CallAction("After starting services", _afterStartingServices);
		}

		void AfterStoppingServices()
		{
			CallAction("After stopping services", _afterStoppingServices);
		}

		void CallAction(string name, Action<IServiceCoordinator> action)
		{
			_log.DebugFormat("[Topshelf] {0}", name);

			if (action != null)
				action(this);

			_log.InfoFormat("[Topshelf] {0} complete", name);
		}
	}
}