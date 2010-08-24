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
	using Magnum.Channels;
	using Magnum.Extensions;
	using Messages;


	public class ServiceController<TService> :
		ServiceStateMachine,
		IServiceController<TService>
		where TService : class
	{
		readonly ILog _log;
		Action<TService> _continueAction;
		TService _instance;
		Action<TService> _pauseAction;
		InternalServiceFactory<TService> _serviceFactory;
		readonly IServiceCoordinator _coordinator;
		Action<TService> _startAction;
		Action<TService> _stopAction;

		public ServiceController(string name, IServiceCoordinator coordinator, UntypedChannel eventChannel,
		                         Action<TService> startAction,
		                         Action<TService> stopAction,
		                         Action<TService> pauseAction,
		                         Action<TService> continueAction,
		                         InternalServiceFactory<TService> serviceFactory)
			: base(name, eventChannel)
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

				Publish<ServiceCreated>();
			}
			catch (Exception ex)
			{
				throw new BuildServiceException(Name, typeof(TService), ex);
			}
		}

		protected override void ServiceCreated(ServiceCreated message)
		{
		}

		protected override void Start()
		{
			CallAction("Start", _startAction);

			Publish<ServiceRunning>();
		}

		protected override void Unload()
		{
			var disposableInstance = _instance as IDisposable;
			if (disposableInstance != null)
			{
				using (disposableInstance)
					_log.DebugFormat("[{0}] Disposing of instance", Name);
			}

			_instance = null;

			Publish<ServiceUnloaded>();
		}

		protected override void Stop()
		{
			CallAction("Stop", _stopAction);

			Publish<ServiceStopped>();
		}

		protected override void Pause()
		{
			CallAction("Pause", _pauseAction);

			Publish<ServicePaused>();
		}

		protected override void Continue()
		{
			CallAction("Continue", _continueAction);

			Publish<ServiceRunning>();
		}

		void CallAction(string text, Action<TService> callback)
		{
			if (callback == null)
				return;

			_log.DebugFormat("[{0}] {1}", Name, text);

			callback(_instance);

			_log.InfoFormat("[{0}] {1} complete", Name, text);
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