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
namespace Topshelf.Shelving
{
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using log4net;
	using Magnum.Channels;
	using Magnum.Extensions;
	using Magnum.Reflection;
	using Magnum.StateMachine;
	using Messages;


	[DebuggerDisplay("{ShelfName}: {CurrentState}")]
	public class ShelfService :
		StateMachine<ShelfService>
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(ShelfService));

		readonly UntypedChannel _eventChannel;
		AssemblyName[] _assemblyNames;
		Type _bootstrapperType;
		ShelfReference _reference;
		ShelfType _shelfType;

		static ShelfService()
		{
			Define(() =>
				{
					Initially(
					          When(OnCreate)
					          	.Call((instance, message) => instance.Initialize(message))
					          	.TransitionTo(Creating));

					During(Creating,
					       When(OnShelfCreated)
					       	.Call((instance, message) => instance.ShelfCreated(message))
					       	.TransitionTo(Created));

					During(Created,
					       When(OnStart)
					       	.Call((instance, message) => instance.StartShelfService())
					       	.TransitionTo(Starting));

					During(Running,
					       When(OnStop)
					       	.Call((instance, message) => instance.StopShelfService(message)),
							When(OnReload)
							.Call((instance,message) => instance.ReloadShelfService())
							.TransitionTo(Reloading));

					During(Reloading,
					       When(OnServiceStopped)
					       	.Call(instance => instance.UnloadReference())
					       	.Call(instance => instance.CreateShelfReference())
					       	.TransitionTo(Creating));

					Anytime(
					        When(Created.Enter)
					        	.Call(instance => instance.Publish<ServiceCreated>())
					        	.Call(instance => instance.StartShelfService()),
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


		public ShelfService(string name, UntypedChannel eventChannel)
		{
			Name = name;
			_eventChannel = eventChannel;
		}

		public string Name { get; set; }

		public static Event<CreateShelfService> OnCreate { get; set; }
		public static Event<StartService> OnStart { get; set; }
		public static Event<StopService> OnStop { get; set; }
		public static Event<ReloadService> OnReload { get; set; }


		public static Event<ShelfCreated> OnShelfCreated { get; set; }

		public static Event<ServiceStopped> OnServiceStopped { get; set; }


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


//		public HostProxy ShelfChannel
//		{
//			get { return _shelfChannel ?? (_shelfChannel = ShelfChannelBuilder(AppDomain)); }
//		}
//
//		public HostProxy ServiceChannel
//		{
//			get { return _serviceChannel ?? (_serviceChannel = ServiceChannelBuilder(AppDomain)); }
//		}
//
//		public Func<AppDomain, HostProxy> ShelfChannelBuilder { private get; set; }
//		public Func<AppDomain, HostProxy> ServiceChannelBuilder { private get; set; }
//

		void Initialize(CreateShelfService message)
		{
			_log.Debug("Creating shelf service: " + message.ServiceName);

			Name = message.ServiceName;

			_shelfType = message.ShelfType;
			_bootstrapperType = message.BootstrapperType;
			_assemblyNames = message.AssemblyNames;

			CreateShelfReference();
		}

		void CreateShelfReference()
		{
			_reference = new ShelfReference(Name, _shelfType);

			if (_assemblyNames != null)
				_assemblyNames.Each(_reference.LoadAssembly);

			if (_bootstrapperType != null)
				_reference.Create(_bootstrapperType);
			else
				_reference.Create();
		}

		void ShelfCreated(ShelfCreated message)
		{
			_reference.CreateShelfChannel(message.Address, message.PipeName);
		}

		void StartShelfService()
		{
			_log.Debug("Starting shelf: " + Name);
			_reference.ShelfChannel.Send(new StartService
				{
					ServiceName = Name,
				});
		}

		void ReloadShelfService()
		{
			_log.Debug("Reloading shelf: " + Name);

			_reference.ShelfChannel.Send(new StopService
				{
					ServiceName = Name
				});

			// TODO likely need to set a timeout for this operation (mailbox would rock here)
		}

		void UnloadReference()
		{
			_reference.Dispose();
			_reference = null;
		}

		void StopShelfService(StopService message)
		{
			_reference.ShelfChannel.Send(message);
		}

		void Publish<T>()
			where T : ServiceEvent
		{
			T message = FastActivator<T>.Create(Name);

			_eventChannel.Send(message);
		}
	}
}