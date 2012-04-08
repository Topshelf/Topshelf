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
namespace Topshelf.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using Exceptions;
	using Logging;
	using Magnum.Caching;
	using Magnum;
	using Magnum.Extensions;
	using Messages;
	using Stact;
	using Stact.Workflow;


	public class ServiceCoordinator :
		IServiceCoordinator
	{
		static readonly ILog _log = Logger.Get("Topshelf.Model.ServiceCoordinator");
        readonly Cache<string, ActorRef> _actorCache;
		readonly Action<IServiceCoordinator> _afterStartingServices;
		readonly Action<IServiceCoordinator> _afterStoppingServices;
		readonly Action<IServiceCoordinator> _beforeStartingServices;
		readonly ServiceControllerFactory _controllerFactory;
		readonly Fiber _fiber;
		readonly Cache<string, IServiceController> _serviceCache;
		readonly Cache<string, Func<Inbox, IServiceChannel, IServiceController>> _startupServices;
		readonly TimeSpan _timeout;
		readonly AutoResetEvent _updated = new AutoResetEvent(true);
		UntypedChannel _channel;
		ChannelConnection _channelConnection;

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

			_controllerFactory = new ServiceControllerFactory();

			_startupServices = new ConcurrentCache<string, Func<Inbox, IServiceChannel, IServiceController>>();
			_serviceCache = new ConcurrentCache<string, IServiceController>();
            _actorCache = new ConcurrentCache<string, ActorRef>();

			EventChannel = new ChannelAdapter();
		}

		public ServiceCoordinator()
			: this(new PoolFiber(), null, null, null, 1.Minutes())
		{
		}

		public UntypedChannel EventChannel { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Send<T>(T message)
		{
			if (_channel == null)
				throw new InvalidOperationException("The service coordinator must be started before sending it any messages");

			_channel.Send(message);
		}

		public void Start()
		{
			AppDomain.CurrentDomain.UnhandledException += UnhandledException;

			CreateCoordinatorChannel();

			BeforeStartingServices();

			_startupServices.Each((name, builder) =>
				{
					ActorFactory<IServiceController> factory = _controllerFactory.CreateFactory(inbox =>
						{
							IServiceController controller = builder(inbox, this);
							_serviceCache.Add(name, controller);

							return controller;
						});

                    ActorRef instance = factory.GetActor();
					_actorCache.Add(name, instance);

					instance.Send(new CreateService(name));
				});

			WaitUntilServicesAreRunning(_startupServices.GetAllKeys(), _timeout);

			AfterStartingServices();
		}

		public void CreateService(string serviceName, Func<Inbox, IServiceChannel, IServiceController> serviceFactory)
		{
			_startupServices.Add(serviceName, serviceFactory);
		}

		public void Stop()
		{
			_stopping = true;

			SendStopCommandToServices();

			WaitUntilAllServicesAre(_controllerFactory.Workflow.GetState(x => x.Completed), _timeout);

			AfterStoppingServices();
		}

		void UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var originatingAppDomain = sender as AppDomain;
			string serviceName = "Topshelf";

			// prefer cast over 'as' keyword if not checking for null,
			var ex = (Exception)e.ExceptionObject;

			if (originatingAppDomain != null)
			{
				serviceName = originatingAppDomain.FriendlyName;

				// TODO: Give fuller exception details in the fault message; 
				// the exception arguments contains more information than we are sending
				var exception =
					new TopshelfException("An unhandled exception occurred within the service: " + serviceName + Environment.NewLine
					                      + ex.Message + ex.StackTrace);

				_channel.Send(new ServiceFault(originatingAppDomain.FriendlyName, exception));
			}

			_log.Fatal("An unhandled exception occurred within the service: " + serviceName, ex);
		}

		void CreateCoordinatorChannel()
		{
			if (_channel != null)
				return;

			_channel = new ChannelAdapter();
			_channelConnection = _channel.Connect(x =>
				{
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

					x.AddConsumerOf<ServiceFolderRemoved>()
						.UsingConsumer(OnServiceFolderRemoved)
						.HandleOnFiber(_fiber);

					x.AddConsumerOf<Request<ServiceStatus>>()
						.UsingConsumer(Status)
						.HandleOnFiber(_fiber);

				    x.AddConsumerOf<StopService>()
				        .UsingConsumer(OnServiceStop)
				        .HandleOnFiber(_fiber);

                    x.AddConsumerOf<StartService>()
                        .UsingConsumer(OnServiceStart)
                        .HandleOnFiber(_fiber);

                    x.AddConsumerOf<UnloadService>()
                        .UsingConsumer(OnServiceUnload)
                        .HandleOnFiber(_fiber);
                });
		}

		void WaitUntilServicesAreRunning(IEnumerable<string> services, TimeSpan timeout)
		{
			DateTime stopTime = SystemUtil.Now + timeout;

			State running = _controllerFactory.Workflow.GetState(s => s.Running);

			while (SystemUtil.Now < stopTime)
			{
				_updated.WaitOne(1.Seconds());

				bool success = services
				               	.Where(key => _serviceCache.Has(key))
				               	.Select(key => _serviceCache[key])
				               	.Count(x => x.CurrentState == running)
				               == services.Count();
				if (success)
					return;

				bool anyFailed = services
					.Where(key => _serviceCache.Has(key))
					.Select(key => _serviceCache[key])
					.Any(service => service.CurrentState == _controllerFactory.Workflow.GetState(s => s.Faulted));

				if (anyFailed)
					throw new TopshelfException("At least one configured service failed to start");
			}

			throw new TopshelfException("All services were not started within the specified timeout");
		}

		void OnCreateShelfService(CreateShelfService message)
		{
			_log.InfoFormat("[Topshelf] Create Shelf Service: {0}{1}", message.ServiceName,
			                message.BootstrapperType == null
			                	? ""
			                	: " ({0})".FormatWith(message.BootstrapperType.ToShortTypeName()));

			ActorFactory<IServiceController> factory = _controllerFactory.CreateFactory(inbox =>
				{
					IServiceController controller = new ShelfServiceController(inbox, message.ServiceName, this, message.ShelfType,
					                                                           message.BootstrapperType, message.AssemblyNames);
					_serviceCache.Add(message.ServiceName, controller);

					return controller;
				});

            ActorRef instance = factory.GetActor();
			_actorCache.Add(message.ServiceName, instance);

			instance.Send(new CreateService(message.ServiceName));
		}

		void OnServiceFolderChanged(ServiceFolderChanged message)
		{
			_log.InfoFormat("[Topshelf] Folder Changed: {0}", message.ServiceName);

			if (_actorCache.Has(message.ServiceName))
				_actorCache[message.ServiceName].Send(new RestartService(message.ServiceName));
			else
				OnCreateShelfService(new CreateShelfService(message.ServiceName, ShelfType.Folder, null, new AssemblyName[] {}));
		}

        void OnServiceFolderRemoved(ServiceFolderRemoved message)
        {
            _log.InfoFormat("[Topshelf] Folder Removed: {0}", message.ServiceName);

            OnServiceUnload(new UnloadService(message.ServiceName));
        }

        void OnServiceUnload(UnloadService message)
		{
            _log.InfoFormat("[Topshelf] Unloading service: {0}", message.ServiceName);

			if (_actorCache.Has(message.ServiceName))
			{
				var actor = _actorCache[message.ServiceName];

				_actorCache.Remove(message.ServiceName);
				_serviceCache.Remove(message.ServiceName);

				ChannelConnection connection = null;
				connection = _channel.Connect(x =>
					{
						x.AddConsumerOf<ServiceStopped>()
							.Where(m => m.ServiceName == message.ServiceName)
							.UsingConsumer(_ =>
								{
									actor.Send(new UnloadService(message.ServiceName));
								});

						x.AddConsumerOf<ServiceUnloaded>()
							.Where(m => m.ServiceName == message.ServiceName)
							.UsingConsumer(_ =>
								{
									// actor.Exit(); why timeout?
									connection.Dispose();
								});
					});

				actor.Send(new StopService(message.ServiceName));
			}
		}

        void OnServiceStop(StopService message)
		{
            _log.InfoFormat("[Topshelf] Stopping service: {0}", message.ServiceName);

			if (_actorCache.Has(message.ServiceName))
			{
                _actorCache[message.ServiceName].Send(new StopService(message.ServiceName));
            }
        }

        void OnServiceStart(StartService message)
        {
            _log.InfoFormat("[Topshelf] Starting service: {0}", message.ServiceName);

            if (_actorCache.Has(message.ServiceName))
            {
                _actorCache[message.ServiceName].Send(new RestartService(message.ServiceName));
            }
        }

		void OnServiceFault(ServiceFault message)
		{
			_log.ErrorFormat("Fault on {0}: {1}", message.ServiceName, message.ToLogString());

			if (message.ServiceName == AppDomain.CurrentDomain.FriendlyName)
			{
				// we caught an unhandled exception, so what should we do? How about restarting all the services?

				if (_stopping == false)
					_actorCache.Each((name, x) => x.Send(new RestartService(name)));
			}

			if (_stopping)
			{
				if (_actorCache.Has(message.ServiceName))
					_actorCache[message.ServiceName].Send(new UnloadService(message.ServiceName));
			}

			EventChannel.Send(message);
		}

		void WaitUntilAllServicesAre(State state, TimeSpan timeout)
		{
			DateTime stopTime = SystemUtil.Now + timeout;

			while (SystemUtil.Now < stopTime)
			{
				_updated.WaitOne(1.Seconds());

				if (GetServicesNotInState(state).Count() == 0)
					break;
			}

			if (GetServicesNotInState(state).Count() > 0)
			{
				GetServicesNotInState(state).Each(x => _log.ErrorFormat("[{0}] Failed to stop", x.Name));

				throw new InvalidOperationException(
					"All services were not {0} within the specified timeout".FormatWith(state.Name));
			}
		}

		void Status(Request<ServiceStatus> request)
		{
			ServiceInfo[] serviceInfos = _serviceCache
				.Select(sc => new ServiceInfo(sc.Name, sc.CurrentState.Name, sc.ServiceType.Name))
				.ToArray();

			var status = new ServiceStatus(serviceInfos);

			request.Respond(status);
		}

		void OnServiceEvent(ServiceEvent message)
		{
			_log.InfoFormat("({0}) {1}", message.ServiceName, message.EventType);
			_updated.Set();
		}

		void OnServiceStopped(ServiceStopped message)
		{
			if (_stopping)
				_actorCache[message.ServiceName].Send(new UnloadService(message.ServiceName));

			EventChannel.Send(message);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_channelConnection != null)
				{
					_log.DebugFormat("[Topshelf] Closing coordinator channel");
					_channelConnection.Dispose();
					_channelConnection = null;
				}

                if(_updated != null && _updated.Implements<IDisposable>())
                {
                    var disp = (IDisposable)_updated;
                    disp.Dispose();
                }

				_channel = null;
			}

			_disposed = true;
		}

		IEnumerable<IServiceController> GetServicesNotInState(State expected)
		{
			return _serviceCache
				.Where(x => x.CurrentState != expected);
		}

		void SendStopCommandToServices()
		{
			_serviceCache.Each((name, service) =>
				{
					var message = new StopService(name);

					_actorCache[name].Send(message);
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