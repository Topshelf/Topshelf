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
	using System.Diagnostics;
	using Exceptions;
	using Internal;
	using Logging;
	using Magnum.Extensions;
	using Messages;
	using Stact;
	using Stact.Workflow;

    /// <summary>
    /// Control a service resident in 'this' domain.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    [DebuggerDisplay("{DebuggerDisplay()}")]
	public class LocalServiceController<TService> :
		IServiceController<TService>
		where TService : class
	{
		readonly IServiceChannel _coordinatorChannel;
		readonly ILog _log;
		readonly string _name;
		readonly Inbox _inbox;
		Action<TService> _continueAction;
		bool _disposed;
		TService _instance;
		Action<TService> _pauseAction;
		InternalServiceFactory<TService> _serviceFactory;
		Action<TService> _startAction;
		Action<TService> _stopAction;


		public LocalServiceController(string name,
			Inbox inbox,
		                              IServiceChannel coordinatorChannel,
		                              Action<TService> startAction,
		                              Action<TService> stopAction,
		                              Action<TService> pauseAction,
		                              Action<TService> continueAction,
		                              InternalServiceFactory<TService> serviceFactory)
		{
			_name = name;
			_inbox = inbox;
			_coordinatorChannel = coordinatorChannel;
			_startAction = startAction;
			_continueAction = continueAction;
			_serviceFactory = serviceFactory;
			_pauseAction = pauseAction;
			_stopAction = stopAction;

			_log = Logger.Get("Topshelf.Host.Service." + name);
		}

		public Type ServiceType
		{
			get { return typeof(TService); }
		}

		public State CurrentState { get; set; }

		public string Name
		{
			get { return _name; }
		}

		public void Create()
		{
			try
			{
				_log.DebugFormat("[{0}] Create", _name);

				_instance = _serviceFactory(_name, _coordinatorChannel);

				if (_instance == null)
				{
					throw new TopshelfException("The service instance returned was null for service type "
					                                 + typeof(TService).ToShortTypeName());
				}

				_log.InfoFormat("[{0}] Create complete", _name);

				Publish(new ServiceCreated(_name));
			}
			catch (Exception ex)
			{
				Publish(new ServiceFault(_name, new BuildServiceException(_name, typeof(TService), ex)));
			}
		}

		public void Start()
		{
			CallAction("Start", _startAction, () => new ServiceStarting(_name), () => new ServiceRunning(_name));
		}

		public void Stop()
		{
			CallAction("Stop", _stopAction, () => new ServiceStopping(_name), () => new ServiceStopped(_name));
		}

		public void Pause()
		{
			CallAction("Pause", _pauseAction, () => new ServicePausing(_name), () => new ServicePaused(_name));
		}

		public void Continue()
		{
			CallAction("Continue", _continueAction, () => new ServiceContinuing(_name), () => new ServiceRunning(_name));
		}

		public void Unload()
		{
			CallAction("Unload", instance =>
				{
					var disposable = instance as IDisposable;
					if (disposable != null)
					{
						using (disposable)
							_log.DebugFormat("[{0}] Dispose", _name);
					}
				}, () => new ServiceUnloading(_name), () => new ServiceUnloaded(_name));

			_instance = null;
		}

	    void CallAction<TBefore, TComplete>(string text, Action<TService> callback, Func<TBefore> before,
		                                    Func<TComplete> complete)
			where TComplete : ServiceEvent
			where TBefore : ServiceEvent
		{
			if (callback == null)
				return;

			try
			{
				_log.DebugFormat("[{0}] {1}", _name, text);

				Publish(before());

				callback(_instance);

				_log.InfoFormat("[{0}] {1} complete", _name, text);

				Publish(complete());
			}
			catch (Exception ex)
			{
				Publish(new ServiceFault(_name, ex));
			}
		}


	    void Publish<T>(T message)
		{
			_coordinatorChannel.Send(message);
			_inbox.Send(message);
		}

        [UsedImplicitly]
        string DebuggerDisplay()
        {
            return "Local Service:{0}".FormatWith(_name);
        }
	    public void Dispose()
	    {
	        Dispose(true);
	        GC.SuppressFinalize(this);
	    }

	    void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_startAction = null;
				_stopAction = null;
				_pauseAction = null;
				_continueAction = null;
				_serviceFactory = null;
			}

			_disposed = true;
		}
	}
}