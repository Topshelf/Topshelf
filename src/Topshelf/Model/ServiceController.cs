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
	using Magnum.Channels;
	using Magnum.Extensions;
	using Messages;


	public class ServiceController<TService> :
		ServiceStateMachine,
		IService<TService>
		where TService : class
	{
		ServiceBuilder _buildAction;
		Action<TService> _continueAction;
		TService _instance;
		Action<TService> _pauseAction;
		Action<TService> _startAction;
		Action<TService> _stopAction;

		public ServiceController(string name, UntypedChannel eventChannel, Action<TService> startAction, Action<TService> stopAction,
		               Action<TService> pauseAction, Action<TService> continueAction, ServiceBuilder buildAction)
			: base(name, eventChannel)
		{
			_startAction = startAction;
			_continueAction = continueAction;
			_buildAction = buildAction;
			_pauseAction = pauseAction;
			_stopAction = stopAction;
		}

		public Type ServiceType
		{
			get { return typeof(TService); }
		}

		protected override void Create(CreateService message)
		{
			Create();
		}

		protected override void Create()
		{
			try
			{
				_instance = (TService)_buildAction(Name);

				if (_instance == null)
				{
					throw new NullReferenceException("The service instance returned was null for service type "
					                                 + typeof(TService).ToShortTypeName());
				}
			}
			catch (Exception ex)
			{
				throw new BuildServiceException(Name, typeof(TService), ex);
			}
		}

		protected override void Start()
		{
			_startAction(_instance);
		}

		protected override void Unload()
		{
		}

		protected override void Stop(StopService message)
		{
			_stopAction(_instance);
		}

		public void Pause(PauseService message)
		{
			_pauseAction(_instance);
		}

		public void Continue(ContinueService message)
		{
			_continueAction(_instance);
		}

		protected override void ServiceCreated(ServiceCreated message)
		{
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
				_buildAction = null;
			}
		}
	}


//	[DebuggerDisplay("Service({Name}) is {State}")]
//	public class ServiceControllexxxxr<TService> :
//		StateMachine<ServiceController<TService>>,
//		IServiceController
//		where TService : class
//	{
//		readonly OutboundChannel _coordinatorChannel;
//		InboundChannel _channel;
//		bool _created;
//		bool _disposed;
//		Fiber _fiber;
//
//
//
//		public ServiceController(string serviceName, OutboundChannel coordinatorChannel)
//		{
//			Name = serviceName;
//
//			_fiber = new ThreadPoolFiber();
//			_coordinatorChannel = coordinatorChannel;
//
//			_channel = WellknownAddresses.CreateServiceChannel(serviceName, s =>
//				{
//					s.AddConsumersFor<ServiceController<TService>>()
//						.UsingInstance(this)
//						.ExecuteOnFiber(_fiber);
//				});
//		}
//
//	
//
//		public void Dispose()
//		{
//			Dispose(true);
//			GC.SuppressFinalize(this);
//		}
//
//		public void Send<T>(T message)
//		{
//			_channel.Send(message);
//		}
//
//		public string Name { get; private set; }
//
//		public Type ServiceType
//		{
//			get { return typeof(TService); }
//		}
//
//		public ServiceState State
//		{
//			get { return (ServiceState)Enum.Parse(typeof(ServiceState), CurrentState.Name, true); }
//		}
//
//		protected void Dispose(bool disposing)
//		{
//			if (!disposing)
//				return;
//			if (_disposed)
//				return;
//
//			if (_channel != null)
//			{
//				_channel.Dispose();
//				_channel = null;
//			}
//
//			_instance = default(TService);
//
//			_fiber.Shutdown(30.Seconds());
//
//			StartAction = null;
//			StopAction = null;
//			PauseAction = null;
//			ContinueAction = null;
//			BuildService = null;
//			_disposed = true;
//		}
//
//		~ServiceController()
//		{
//			Dispose(false);
//		}
//
//
//	}
}