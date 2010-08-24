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
	using System.Linq;
	using System.Threading;
	using log4net;
	using Magnum;
	using Magnum.Channels;
	using Magnum.Collections;
	using Magnum.Extensions;
	using Magnum.Fibers;
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

			_serviceCache = new Cache<string, ServiceStateMachine>(key => new ServiceStateMachine(key, _channel));

			_channel = AddressRegistry.GetServiceCoordinatorHost(x =>
				{
					x.AddConsumersFor<ServiceStateMachine>()
						.BindUsing<ServiceStateMachineBinding, string>()
						.ExecuteOnThreadPoolFiber()
						.PersistInMemoryUsing(_serviceCache);

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

		public void Start(TimeSpan timeout)
		{
			BeforeStartingServices();

			throw new NotImplementedException();

			AfterStartingServices();
		}

		/// <summary>
		/// Creates a shelf service using the specified bootstrapper type
		/// </summary>
		/// <param name="serviceName">The name of the service to create</param>
		/// <param name="bootstrapperType">The type of the bootstrapper class for the service</param>
		public void CreateShelfService(string serviceName, Type bootstrapperType)
		{
			_channel.Send(new CreateShelfService(serviceName, ShelfType.Internal, bootstrapperType));
		}

		/// <summary>
		/// Creates a shelf service by name, determining the bootstrapper type by reflection
		/// </summary>
		/// <param name="serviceName">The name of the service to create (should match the folder)</param>
		public void CreateShelfService(string serviceName)
		{
			_channel.Send(new CreateShelfService(serviceName, ShelfType.Folder));
		}

		public void Send<T>(T message)
		{
			_channel.Send(message);
		}

		~ServiceCoordinator()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				StopServices(10.Minutes());

				if (_channel != null)
				{
					_channel.Dispose();
					_channel = null;
				}
			}

			_disposed = true;
		}

		void OnServiceStopped(ServiceStopped message)
		{
			if (_stopping)
				_channel.Send(new UnloadService(message.ServiceName));

			_updated.Set();
		}

		void StopServices(TimeSpan timeout)
		{
			_stopping = true;

			SendStopCommandToServices();

			DateTime stopTime = SystemUtil.Now + timeout;

			while (SystemUtil.Now < stopTime)
			{
				_updated.WaitOne(1.Seconds());

				if (AllServicesAreCompleted())
					break;
			}

			if (!AllServicesAreCompleted())
				throw new InvalidOperationException("The services did not stop without the specified timeout");

			AfterStoppingServices();
		}

		bool AllServicesAreCompleted()
		{
			return !_serviceCache.Any(x => x.CurrentState != ServiceStateMachine.Completed);
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
			CallAction("before starting services", _beforeStartingServices);
		}

		void AfterStartingServices()
		{
			CallAction("after starting services", _afterStartingServices);
		}

		void AfterStoppingServices()
		{
			CallAction("after stopping services", _afterStoppingServices);
		}

		void CallAction(string name, Action<IServiceCoordinator> action)
		{
			_log.DebugFormat("Calling {0} action", name);

			if (action != null)
				action(this);

			_log.InfoFormat("Call to {0} complete", name);
		}
	}
}