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
	using System.Reflection;
	using System.Threading;
	using Exceptions;
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
		readonly Cache<string, Func<IServiceCoordinator, ServiceStateMachine>> _startupServices;
		readonly TimeSpan _timeout;
		readonly AutoResetEvent _updated = new AutoResetEvent(true);
		InboundChannel _channel;

		volatile bool _disposed;

		volatile bool _stopping;

		public ServiceCoordinator(Fiber fiber,
		                          Action<IServiceCoordinator> beforeStartingServices,
		                          Action<IServiceCoordinator> afterStartingServices,
		                          Action<IServiceCoordinator> afterStoppingServices,
		                          TimeSpan timeout)
		{
			_fiber = fiber;
			_afterStoppingServices = afterStoppingServices;
			_afterStartingServices = afterStartingServices;
			_beforeStartingServices = beforeStartingServices;
			_timeout = timeout;

			_startupServices = new Cache<string, Func<IServiceCoordinator, ServiceStateMachine>>();
			_serviceCache = new Cache<string, ServiceStateMachine>();

			EventChannel = new ChannelAdapter();
		}

		public ServiceCoordinator()
			: this(new ThreadPoolFiber(), null, null, null, 1.Minutes())
		{
		}

		public UntypedChannel EventChannel { get; private set; }

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

		public void Send<T>(T message)
		{
			if (_channel == null)
				throw new InvalidOperationException("The service coordinator must be started before sending it any messages");

			_channel.Send(message);
		}

		public void Start()
		{
			CreateCoordinatorChannel();

			BeforeStartingServices();

			string[] servicesToStart = _startupServices.GetAllKeys();

			servicesToStart.Each(name => _channel.Send(new CreateService(name, _channel.Address, _channel.PipeName)));

			WaitUntilServicesAreRunning(servicesToStart, _timeout);

			AfterStartingServices();
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

		public void Stop()
		{
			_stopping = true;

			SendStopCommandToServices();

			WaitUntilAllServicesAre(ServiceStateMachine.Completed, _timeout);

			AfterStoppingServices();
		}

		void CreateCoordinatorChannel()
		{
			if (_channel != null)
				return;

			_channel = AddressRegistry.GetInboundServiceCoordinatorChannel(x =>
				{
					x.AddConsumersFor<ServiceStateMachine>()
						.BindUsing<ServiceStateMachineBinding, string>()
						.CreateNewInstanceBy(GetServiceInstance)
						.HandleOnInstanceFiber()
						.PersistInMemoryUsing(_serviceCache);

					x.AddConsumerOf<ServiceEvent>()
						.UsingConsumer(OnServiceEvent)
						.HandleOnFiber(_fiber);

					x.AddConsumerOf<ServiceFault>()
						.UsingConsumer(OnServiceFault)
						.HandleOnFiber(_fiber);

					x.AddConsumerOf<ServiceStopped>()
						.UsingConsumer(OnServiceStopped)
						.HandleOnFiber(_fiber);

					x.AddConsumerOf<CreateShelfService>()
						.UsingConsumer(OnCreateShelfService)
						.HandleOnFiber(_fiber);

					x.AddConsumerOf<ServiceFolderChanged>()
						.UsingConsumer(OnServiceFolderChanged)
						.HandleOnFiber(_fiber);
				});
		}

		void WaitUntilServicesAreRunning(IEnumerable<string> services, TimeSpan timeout)
		{
			DateTime stopTime = SystemUtil.Now + timeout;

			while (SystemUtil.Now < stopTime)
			{
				_updated.WaitOne(1.Seconds());

				bool success = services
				               	.Where(key => _serviceCache.Has(key))
				               	.Select(key => _serviceCache[key])
				               	.Count(x => x.CurrentState == ServiceStateMachine.Running) == services.Count();
				if (success)
					return;

				bool anyFailed = services
					.Where(key => _serviceCache.Has(key))
					.Select(key => _serviceCache[key])
					.Any(service => service.CurrentState == ServiceStateMachine.Faulted);

				if (anyFailed)
					throw new TopshelfException("At least one configured service failed to start");
			}

			throw new TopshelfException("All services were not started within the specified timeout");
		}

		void OnCreateShelfService(CreateShelfService message)
		{
			_log.InfoFormat("[Topshelf] Received shelf request for {0}{1}", message.ServiceName,
			                message.BootstrapperType == null
			                	? ""
			                	: " ({0})".FormatWith(message.BootstrapperType.ToShortTypeName()));

			_startupServices.Add(message.ServiceName,
			                     x => new ShelfServiceController(message.ServiceName, _channel, message.ShelfType,
			                                                     message.BootstrapperType, message.AssemblyNames));

			_channel.Send(new CreateService(message.ServiceName));
		}

		void OnServiceFolderChanged(ServiceFolderChanged message)
		{
			_log.InfoFormat("[Topshelf] Folder Changed: {0}", message.ServiceName);

			if (_serviceCache.Has(message.ServiceName))
				_channel.Send(new RestartService(message.ServiceName));
			else
			{
				_startupServices.Add(message.ServiceName,
				                     x => new ShelfServiceController(message.ServiceName, _channel, ShelfType.Folder,
				                                                     null, new AssemblyName[] {}));

				_channel.Send(new CreateService(message.ServiceName));
			}
		}

		void OnServiceFault(ServiceFault message)
		{
			_log.ErrorFormat("Fault on {0}: {1}", message.ServiceName, message.ExceptionMessage);

			EventChannel.Send(message);
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
			{
				_serviceCache.Where(x => x.CurrentState != state).Each(x => _log.ErrorFormat("[{0}] Failed to stop", x.Name));

				throw new InvalidOperationException("All services were not {0} within the specified timeout".FormatWith(state.Name));
			}
		}

		ServiceStateMachine GetServiceInstance(string key)
		{
			if (key == null)
				return new ServiceStateMachine(null, _channel);

			if (_startupServices.Has(key))
				return _startupServices[key](this);

			_log.WarnFormat("[Topshelf] No factory for service {0}", key);
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

			EventChannel.Send(message);
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