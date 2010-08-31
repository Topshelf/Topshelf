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
	using Exceptions;
	using log4net;
	using Magnum.Extensions;
	using Messages;


	public class ServiceController<TService> :
		ServiceStateMachine,
		IServiceController<TService>
		where TService : class
	{
		readonly IServiceCoordinator _coordinator;
		readonly ILog _log;
		Action<TService> _continueAction;
		TService _instance;
		Action<TService> _pauseAction;
		InternalServiceFactory<TService> _serviceFactory;
		Action<TService> _startAction;
		Action<TService> _stopAction;
		Uri _address;
		string _pipeName;

		public ServiceController(string name, IServiceCoordinator coordinator, ServiceChannel coordinatorChannel,
		                         Action<TService> startAction,
		                         Action<TService> stopAction,
		                         Action<TService> pauseAction,
		                         Action<TService> continueAction,
		                         InternalServiceFactory<TService> serviceFactory)
			: base(name, coordinatorChannel)
		{
			_coordinator = coordinator;
			_startAction = startAction;
			_continueAction = continueAction;
			_serviceFactory = serviceFactory;
			_pauseAction = pauseAction;
			_stopAction = stopAction;

			_log = LogManager.GetLogger("Topshelf.Host.Service." + name);
		}

		public Type ServiceType
		{
			get { return typeof(TService); }
		}

		protected override void Create(CreateService message)
		{
			_address = message.Address;
			_pipeName = message.PipeName;

			Create();
		}

		protected override void Create()
		{
			try
			{
				_log.DebugFormat("[{0}] Creating service", Name);

				_instance = _serviceFactory(Name, _coordinator);

				if (_instance == null)
				{
					throw new NullReferenceException("The service instance returned was null for service type "
					                                 + typeof(TService).ToShortTypeName());
				}

				Publish(new ServiceCreated(Name, _address, _pipeName));
			}
			catch (Exception ex)
			{
				Publish(new ServiceFault(Name, new BuildServiceException(Name, typeof(TService), ex)));
			}
		}

		protected override void ServiceCreated(ServiceCreated message)
		{
		}

		protected override void ServiceFaulted(ServiceFault message)
		{
			_log.ErrorFormat("[{0}] Faulted: {1}", Name, message.ExceptionMessage);
		}

		protected override void Start()
		{
			CallAction<ServiceStarting, ServiceRunning>("Start", _startAction);
		}

		protected override void Stop()
		{
			CallAction<ServiceStopping, ServiceStopped>("Stop", _stopAction);
		}

		protected override void Pause()
		{
			CallAction<ServicePausing, ServicePaused>("Pause", _pauseAction);
		}

		protected override void Continue()
		{
			CallAction<ServiceContinuing,ServiceRunning>("Continue", _continueAction);
		}

		protected override void Unload()
		{
			CallAction<ServiceUnloading, ServiceUnloaded>("Unload", instance =>
				{
					var disposable = instance as IDisposable;
					if (disposable != null)
					{
						using (disposable)
							_log.DebugFormat("[{0}] Dispose", Name);
					}
				});

			_instance = null;
		}

		void CallAction<TBefore, TComplete>(string text, Action<TService> callback)
			where TComplete : ServiceEvent
			where TBefore : ServiceEvent
		{
			if (callback == null)
				return;

			try
			{
				_log.DebugFormat("[{0}] {1}", Name, text);

				Publish<TBefore>();

				callback(_instance);

				_log.InfoFormat("[{0}] {1} complete", Name, text);

				Publish<TComplete>();
			}
			catch (Exception ex)
			{
				Publish(new ServiceFault(Name, ex));
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_startAction = null;
				_stopAction = null;
				_pauseAction = null;
				_continueAction = null;
				_serviceFactory = null;
			}
		}
	}
}