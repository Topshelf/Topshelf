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
	using log4net;
	using Magnum.Channels;
	using Magnum.Extensions;
	using Magnum.Fibers;
	using Magnum.Reflection;
	using Magnum.StateMachine;
	using Messages;
	using Shelving;


	[DebuggerDisplay("Service({Name}) is {State}")]
	public class ServiceController<TService> :
		StateMachine<ServiceController<TService>>,
		IServiceController
		where TService : class
	{
		readonly OutboundChannel _coordinatorChannel;
		InboundChannel _channel;
		bool _created;
		bool _disposed;
		Fiber _fiber;
		TService _instance;
		ILog _log = LogManager.GetLogger(typeof(ServiceController<TService>));

		static ServiceController()
		{
			Define(() =>
				{
					Initially(
					          When(OnCreate)
					          	.Call((instance, message) => instance.Create(),
					          	      InCaseOf<CouldntBuildServiceException>()
					          	      	.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					          	      	.TransitionTo(Failed))
					          	.TransitionTo(Created));

					During(Created,
					       When(OnStart)
					       	.Call((instance, message) => instance.Start(),
					       	      InCaseOf<Exception>()
					       	      	.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					       	      	.TransitionTo(Failed))
					       	.TransitionTo(Running));

					During(Running,
					       When(OnStop)
					       	.Call((instance, message) => instance.Stop())
					       	.TransitionTo(Stopped),
					       When(OnPause)
					       	.Call((instance) => instance.Pause())
					       	.TransitionTo(Paused));

					During(Paused,
					       When(OnContinue)
					       	.Call(instance => instance.Continue())
					       	.TransitionTo(Running),
					       When(OnStop)
					       	.Call(instance => instance.Stop())
					       	.TransitionTo(Stopped));


					Anytime(
					        When(Created.Enter)
					        	.Call(instance => instance.Publish<ServiceCreated>()),
					        When(Running.Enter)
					        	.Call(instance => instance.Publish<ServiceRunning>()),
					        When(Paused.Enter)
					        	.Call(instance => instance.Publish<ServicePaused>()),
					        When(Stopped.Enter)
					        	.Call(instance => instance.Publish<ServiceStopped>())
						);
				});
		}

		public ServiceController(string serviceName, OutboundChannel coordinatorChannel)
		{
			Name = serviceName;

			_fiber = new ThreadPoolFiber();
			_coordinatorChannel = coordinatorChannel;

			_channel = WellknownAddresses.CreateServiceChannel(serviceName, s =>
				{
					s.AddConsumersFor<ServiceController<TService>>()
						.UsingInstance(this)
						.ExecuteOnFiber(_fiber);
				});
		}

		public static Event<CreateService> OnCreate { get; set; }
		public static Event<StartService> OnStart { get; set; }
		public static Event<StopService> OnStop { get; set; }
		public static Event<PauseService> OnPause { get; set; }
		public static Event<ContinueService> OnContinue { get; set; }

		public static State Initial { get; set; }
		public static State Created { get; set; }
		public static State Running { get; set; }
		public static State Paused { get; set; }
		public static State Stopped { get; set; }
		public static State Failed { get; set; }
		public static State Completed { get; set; }

		public Action<TService> StartAction { get; set; }
		public Action<TService> StopAction { get; set; }
		public Action<TService> PauseAction { get; set; }
		public Action<TService> ContinueAction { get; set; }

		public ServiceBuilder BuildService { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Send<T>(T message)
		{
			_channel.Send(message);
		}

		public string Name { get; private set; }

		public Type ServiceType
		{
			get { return typeof(TService); }
		}

		public ServiceState State
		{
			get { return (ServiceState)Enum.Parse(typeof(ServiceState), CurrentState.Name, true); }
		}

		void Publish<T>()
			where T : ServiceEvent
		{
			T message = FastActivator<T>.Create(Name);

			_coordinatorChannel.Send(message);
		}

		void Publish<T>(T message)
		{
			_coordinatorChannel.Send(message);
		}

		protected void Dispose(bool disposing)
		{
			if (!disposing)
				return;
			if (_disposed)
				return;

			if (_channel != null)
			{
				_channel.Dispose();
				_channel = null;
			}

			_instance = default(TService);

			_fiber.Shutdown(30.Seconds());

			StartAction = null;
			StopAction = null;
			PauseAction = null;
			ContinueAction = null;
			BuildService = null;
			_disposed = true;
		}

		~ServiceController()
		{
			Dispose(false);
		}

		void Create()
		{
			try
			{
				_instance = (TService)BuildService(Name);

				if (_instance == null)
					throw new ArgumentNullException("instance", "The service instance returned was null");
			}
			catch (Exception ex)
			{
				throw new CouldntBuildServiceException(Name, typeof(TService), ex);
			}
		}

		public void Start()
		{
			StartAction(_instance);
		}

		public void Stop()
		{
			StopAction(_instance);
		}

		public void Pause()
		{
			PauseAction(_instance);
		}

		public void Continue()
		{
			ContinueAction(_instance);
		}
	}
}