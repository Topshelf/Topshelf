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
	using Magnum.Reflection;
	using Magnum.StateMachine;
	using Messages;


	[DebuggerDisplay("Service: {Name}, State: {CurrentState}")]
	public class ServiceStateMachine :
		StateMachine<ServiceStateMachine>,
		IServiceController
	{
		bool _disposed;

		static ServiceStateMachine()
		{
			Define(() =>
				{
					Initially(
					          When(OnCreate)
					          	.Call((instance, message) => instance.Create(message),
					          	      InCaseOf<BuildServiceException>()
					          	      	.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					          	      	.TransitionTo(Faulted),
					          	      HandleServiceCommandException)
					          	.TransitionTo(Creating)
						);

					During(Creating,
					       When(OnCreated)
					       	.Call((instance, message) => instance.ServiceCreated(message),
					       	      HandleServiceCommandException)
					       	.TransitionTo(Created)
					       	.Call(instance => instance.Start(),
					       	      HandleServiceCommandException)
					       	.TransitionTo(Starting),
					       When(OnFaulted)
					       	.Call((instance, message) => instance.ServiceFaulted(message))
					       	.TransitionTo(Faulted)
						);

					During(Created,
						   When(OnStart)
							.TransitionTo(Starting)
					       	.Call(instance => instance.Start(),
					       	      HandleServiceCommandException),
						   When(OnRunning)
							.TransitionTo(Running)
							.Call(instance => instance.ServiceRunning()));

					During(Starting,
						   When(OnRunning)
							.TransitionTo(Running)
                            .Call(instance => instance.ServiceRunning()),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Running,
						   When(OnStop)
							.TransitionTo(Stopping)
					       	.Call((instance, message) => instance.Stop()),
						   When(OnPause)
							.TransitionTo(Pausing)
					       	.Call((instance, message) => instance.Pause()),
						   When(OnRestart)
							.TransitionTo(StoppingToRestart)
                            .Call((instance, message) => instance.Stop()),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Stopping,
						   When(OnStopped)
							.TransitionTo(Stopped)
                            .Call(instance => instance.ServiceStopped()),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Pausing,
						   When(OnPaused)
							.TransitionTo(Paused)
                            .Call(instance => instance.ServicePaused()),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Paused,
						   When(OnContinue)
							.TransitionTo(Continuing)
                            .Call(instance => instance.Continue()),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Continuing,
						   When(OnRunning)
							.TransitionTo(Running)
                            .Call(instance => instance.ServiceRunning()),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Stopped,
						   When(OnUnload)
							.TransitionTo(Unloading)
					       	.Call(instance => instance.Unload()));

					During(Unloading,
					       When(OnUnloaded)
					       	.TransitionTo(Completed)
					       	.Call(instance => instance.ServiceUnloaded())
					       	.Call(instance => instance.Publish<ServiceCompleted>()));

					During(StoppingToRestart,
					       When(OnStopped)
					       	.Call(instance => instance.Unload())
					       	.Call(instance => instance.Create())
                            .TransitionTo(CreatingToRestart),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(CreatingToRestart,
					       When(OnCreated)
					       	.Call((instance, message) => instance.ServiceCreated(message),
					       	      HandleServiceCommandException)
					       	.Call(instance => instance.Start(),
					       	      HandleServiceCommandException)
                            .TransitionTo(Restarting),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));

					During(Restarting,
					       When(OnRunning)
					       	.Call(instance => instance.Publish<ServiceRestarted>())
                            .TransitionTo(Running),
                           When(OnFaulted)
                            .Call((instance, message) => instance.ServiceFaulted(message))
                            .TransitionTo(Faulted));
				});
		}


		public ServiceStateMachine(string name, ServiceChannel coordinatorChannel)
		{
			Name = name;
			CoordinatorChannel = coordinatorChannel;
		}

		protected ServiceChannel CoordinatorChannel { get; private set; }

		static ExceptionAction<ServiceStateMachine, Exception> HandleServiceCommandException
		{
			get
			{
				return InCaseOf<Exception>()
					.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					.TransitionTo(Faulted);
			}
		}


		public static Event<CreateService> OnCreate { get; set; }
		public static Event<ServiceCreated> OnCreated { get; set; }

		public static Event<StartService> OnStart { get; set; }
		public static Event<ServiceRunning> OnRunning { get; set; }

		public static Event<StopService> OnStop { get; set; }
		public static Event<ServiceStopped> OnStopped { get; set; }

		public static Event<PauseService> OnPause { get; set; }
		public static Event<ServicePaused> OnPaused { get; set; }

		public static Event<ContinueService> OnContinue { get; set; }

		public static Event<UnloadService> OnUnload { get; set; }
		public static Event<ServiceUnloaded> OnUnloaded { get; set; }

		public static Event<RestartService> OnRestart { get; set; }

		public static Event<ServiceFault> OnFaulted { get; set; }


		public static State Initial { get; set; }
		public static State Creating { get; set; }
		public static State Created { get; set; }
		public static State Starting { get; set; }
		public static State Running { get; set; }
		public static State Pausing { get; set; }
		public static State Paused { get; set; }
		public static State Continuing { get; set; }
		public static State Stopping { get; set; }
		public static State Stopped { get; set; }
		public static State StoppingToRestart { get; set; }
		public static State CreatingToRestart { get; set; }
		public static State Restarting { get; set; }
		public static State Unloading { get; set; }
		public static State Completed { get; set; }
		public static State Faulted { get; set; }

		public string Name { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Create(CreateService message)
		{
		}

		protected virtual void Create()
		{
		}

		protected virtual void ServiceCreated(ServiceCreated message)
		{
		}

		protected virtual void ServiceRunning()
		{
		}

		protected virtual void ServiceStopped()
		{
		}

		protected virtual void ServicePaused()
		{
		}

		protected virtual void ServiceUnloaded()
		{
		}

		protected virtual void ServiceFaulted(ServiceFault message)
		{
		}

		protected virtual void Start()
		{
		}

		protected virtual void Pause()
		{
		}

		protected virtual void Continue()
		{
		}

		void Restart(RestartService message)
		{
			// TODO likely need to set a timeout for this operation (mailbox would rock here)
			// TODO isn't this another transaction, with a timeout? )
		}

		protected virtual void Unload()
		{
		}

		protected virtual void Stop()
		{
		}

		protected void Publish<T>()
			where T : ServiceEvent
		{
			T message = FastActivator<T>.Create(Name);

			CoordinatorChannel.Send(message);
		}

		protected void Publish<T>(T message)
		{
			CoordinatorChannel.Send(message);
		}

		~ServiceStateMachine()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
				CoordinatorChannel = null;

			_disposed = true;
		}
	}
}