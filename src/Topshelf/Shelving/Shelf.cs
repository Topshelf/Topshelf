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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using Configuration.Dsl;
	using log4net;
	using log4net.Config;
	using Magnum.Extensions;
	using Magnum.Reflection;
	using Messages;
	using Model;
	using Stact;
	using Stact.Configuration;


	[DebuggerDisplay("Shelf[{ServiceName}]")]
	public class Shelf :
		IDisposable
	{
		readonly Type _bootstrapperType;
		readonly ILog _log;
		readonly string _serviceName;
		InboundChannel _channel;
		OutboundChannel _coordinatorChannel;
		bool _disposed;
		PoolFiber _fiber;
		IServiceController _service;

		public Shelf(Type bootstrapperType, Uri address, string pipeName)
		{
			_bootstrapperType = bootstrapperType;

			BootstrapLogger();

			_serviceName = AppDomain.CurrentDomain.FriendlyName;

			_log = LogManager.GetLogger("Topshelf.Shelf." + _serviceName);

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			_coordinatorChannel = new OutboundChannel(address, pipeName);

			Create();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Shelf()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_channel != null)
				{
					_channel.Dispose();
					_channel = null;
				}

				if (_coordinatorChannel != null)
				{
					_coordinatorChannel.Dispose();
					_coordinatorChannel = null;
				}

				LogManager.Shutdown();
			}

			_disposed = true;
		}

		void Create()
		{
			try
			{
				Type type = FindBootstrapperImplementationType(_bootstrapperType);

				_log.DebugFormat("[{0}] Creating bootstrapper: {1}", _serviceName, type.ToShortTypeName());

				object bootstrapper = FastActivator.Create(type);

				Type serviceType = bootstrapper.GetType()
					.GetInterfaces()
					.First()
					.GetGenericArguments()
					.First();

				_log.DebugFormat("[{0}] Creating configurator for service type: {1}", _serviceName, serviceType.ToShortTypeName());

				object cfg = FastActivator.Create(typeof(ServiceConfigurator<>), new[] { serviceType });

				InitializeAndCreateService(serviceType, bootstrapper, cfg);
			}
			catch (Exception ex)
			{
				SendFault(ex);
			}
		}

		void InitializeAndCreateService(Type serviceType, object bootstrapper, object cfg)
		{
			this.FastInvoke(new[] { serviceType }, "InitializeAndCreateHostedService", bootstrapper, cfg);
		}

		// ReSharper disable UnusedMember.Local
		void InitializeAndCreateHostedService<T>(Bootstrapper<T> bootstrapper, ServiceConfigurator<T> cfg)
			// ReSharper restore UnusedMember.Local
			where T : class
		{
			bootstrapper.FastInvoke("InitializeHostedService", cfg);

			_log.DebugFormat("[{0}] Creating service type: {1}", _serviceName, typeof(T).ToShortTypeName());

			_fiber = new PoolFiber();

			_channel = AddressRegistry.GetInboundServiceChannel(AppDomain.CurrentDomain, AddEventForwarders);


			var controllerFactory = new ServiceControllerFactory();

			ActorFactory<IServiceController> factory = controllerFactory.CreateFactory(inbox =>
				{
					_service = cfg.Create(AppDomain.CurrentDomain.FriendlyName, inbox, _coordinatorChannel);
					return _service;
				});

			ActorInstance instance = factory.GetActor();

			_channel.Connect(x => { x.AddChannel(instance); });

			// this creates the state machine instance in the shelf and tells the servicecontroller
			// to create the service
			_channel.Send(new CreateService(_serviceName));
		}

		void AddEventForwarders(ConnectionConfigurator x)
		{
			// These are needed to ensure that events are updated in the shelf state machine
			// as well as the service coordinator state machine. To handle this, the servicecontroller
			// is given the shelf channel as the reporting channel for events, and the shelf forwards
			// the events to the service coordinator

			x.AddConsumerOf<ServiceCreated>()
				.UsingConsumer(m => _coordinatorChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceRunning>()
				.UsingConsumer(m => _coordinatorChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceStopped>()
				.UsingConsumer(m => _coordinatorChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServicePaused>()
				.UsingConsumer(m => _coordinatorChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceUnloaded>()
				.UsingConsumer(m =>
					{
						_coordinatorChannel.Send(m);
						_log.InfoFormat("[{0}] Unloading Shelf and AppDomain", _serviceName);
						Dispose();
						AppDomain.Unload(AppDomain.CurrentDomain);
					})
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceFault>()
				.UsingConsumer(m => _coordinatorChannel.Send(m))
				.HandleOnFiber(_fiber);
		}

		public static Type FindBootstrapperImplementationType(Type bootstrapper)
		{
			if (bootstrapper != null)
			{
				if (bootstrapper.GetInterfaces().Where(IsBootstrapperType).Count() > 0)
					return bootstrapper;

				throw new InvalidOperationException(
					"Bootstrapper type, '{0}', is not a subclass of Bootstrapper.".FormatWith(bootstrapper.GetType().Name));
			}

			// check configuration first
			ShelfConfiguration config = ShelfConfiguration.GetConfig();
			if (config != null)
				return config.BootstrapperType;

			IEnumerable<Type> possibleTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.Where(x => x.IsInterface == false)
				.Where(t => t.GetInterfaces().Any(IsBootstrapperType));

			if (possibleTypes.Count() > 1)
				throw new InvalidOperationException("Unable to identify the bootstrapper, more than one found.");

			if (possibleTypes.Count() == 0)
				throw new InvalidOperationException("The bootstrapper was not found.");

			return possibleTypes.Single();
		}

		static bool IsBootstrapperType(Type t)
		{
			if (t.IsGenericType)
				return t.GetGenericTypeDefinition() == typeof(Bootstrapper<>);
			return false;
		}

		void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_log.Error("Unhandled {0}exception in app domain {1}: {2}".FormatWith(e.IsTerminating ? "terminal " : "",
																				  AppDomain.CurrentDomain.FriendlyName,
																				  e.ExceptionObject));

			Dispose();
		}

		void SendFault(Exception ex)
		{
			try
			{
				_channel.Send(new ServiceFault(_serviceName, ex));
			}
			catch (Exception)
			{
				_log.Error("[{0}] Failed to send fault".FormatWith(_serviceName), ex);
			}
		}

		static void BootstrapLogger()
		{
			string assemblyPath = Path.GetDirectoryName(typeof(Shelf).Assembly.Location);

			string configurationFilePath = Path.Combine(assemblyPath, "log4net.config");

			var configurationFile = new FileInfo(configurationFilePath);

			XmlConfigurator.ConfigureAndWatch(configurationFile);

			LogManager.GetLogger("Topshelf.Host").DebugFormat("Logging configuration loaded for shelf: {0}",
															  configurationFilePath);
		}
	}
}