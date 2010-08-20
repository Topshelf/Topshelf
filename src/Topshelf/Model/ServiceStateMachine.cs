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
	using log4net;
	using Magnum.Channels;
	using Magnum.Reflection;
	using Magnum.StateMachine;
	using Messages;


	[DebuggerDisplay("{ShelfName}: {CurrentState}")]
	public abstract class ServiceStateMachine<TService, TCreate, TCreated> :
		StateMachine<TService>,
		IDisposable
		where TCreate : ServiceCommand
		where TCreated : ServiceEvent
		where TService : ServiceStateMachine<TService, TCreate, TCreated>
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(ServiceStateMachine<TService, TCreate, TCreated>));

		UntypedChannel _coordinatorChannel;


		static ServiceStateMachine()
		{
			Define(() =>
				{
					Initially(
					          When(OnCreate)
					          	.Call((instance, message) => instance.Create(message),
					          	      InCaseOf<Exception>()
					          	      	.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					          	      	.TransitionTo(Failed))
					          	.TransitionTo(Creating));

					During(Creating,
					       When(OnCreated)
					       	.Call((instance, message) => instance.ServiceCreated(message),
					       	      InCaseOf<Exception>()
					       	      	.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					       	      	.TransitionTo(Failed))
					       	.TransitionTo(Created));

					During(Created,
					       When(OnStart)
					       	.Call((instance, message) => instance.Start(),
					       	      InCaseOf<Exception>()
					       	      	.Then((i, ex) => i.Publish(new ServiceFault(i.Name, ex)))
					       	      	.TransitionTo(Failed))
					       	.TransitionTo(Starting));

					During(Running,
					       When(OnStop)
					       	.Call((instance, message) => instance.Stop(message)),
					       When(OnReload)
					       	.Call((instance, message) => instance.Reload(message))
					       	.TransitionTo(Reloading));

					During(Reloading,
					       When(OnStopped)
					       	.Call(instance => instance.Unload())
					       	.Call(instance => instance.Create())
					       	.TransitionTo(Creating));

					Anytime(
					        When(Created.Enter)
					        	.Call(instance => instance.Publish<ServiceCreated>())
					        	.Call(instance => instance.Start()),
					        When(Starting.Enter)
					        	.Call(instance => instance.Publish<ServiceStarting>()),
					        When(Running.Enter)
					        	.Call(instance => instance.Publish<ServiceRunning>()),
					        When(Pausing.Enter)
					        	.Call(instance => instance.Publish<ServicePausing>()),
					        When(Paused.Enter)
					        	.Call(instance => instance.Publish<ServicePaused>()),
					        When(Continuing.Enter)
					        	.Call(instance => instance.Publish<ServiceContinuing>()),
					        When(Stopping.Enter)
					        	.Call(instance => instance.Publish<ServiceStopping>()),
					        When(Stopped.Enter)
					        	.Call(instance => instance.Publish<ServiceStopped>())
						);
				});
		}


		public ServiceStateMachine(string name, UntypedChannel coordinatorChannel)
		{
			Name = name;
			_coordinatorChannel = coordinatorChannel;
		}

		public string Name { get; set; }

		public static Event<TCreate> OnCreate { get; set; }
		public static Event<TCreated> OnCreated { get; set; }

		public static Event<StartService> OnStart { get; set; }
		public static Event<ReloadService> OnReload { get; set; }

		public static Event<StopService> OnStop { get; set; }
		public static Event<ServiceStopped> OnStopped { get; set; }


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
		public static State Reloading { get; set; }
		public static State Completed { get; set; }
		public static State Failed { get; set; }


		protected abstract void Create(TCreate message);
		protected abstract void Create();
		protected abstract void ServiceCreated(TCreated message);


		protected abstract void Start();

		protected virtual void Reload(ReloadService message)
		{
			Stop(new StopService
				{
					ServiceName = message.ServiceName
				});

			// TODO likely need to set a timeout for this operation (mailbox would rock here)
		}

		protected abstract void Unload();

		protected abstract void Stop(StopService message);

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

		bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
			{
				_coordinatorChannel = null;
			}

			_disposed = true;
		}
	}
}