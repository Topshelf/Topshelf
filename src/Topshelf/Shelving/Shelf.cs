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
	using Magnum.Channels;
	using Magnum.Collections;
	using Magnum.Extensions;
	using Magnum.Reflection;
	using Messages;
	using Model;


	[DebuggerDisplay("Shelf[{ServiceName}]")]
	public class Shelf :
		IDisposable
	{
		readonly Type _bootstrapperType;
		OutboundChannel _coordinatorChannel;
		readonly ILog _log;
		readonly string _serviceName;
		InboundChannel _channel;
		bool _disposed;
		ServiceStateMachine _service;

		public Shelf(Type bootstrapperType)
		{
			_bootstrapperType = bootstrapperType;

			BootstrapLogger();

			_log = LogManager.GetLogger(typeof(Shelf));

			_serviceName = AppDomain.CurrentDomain.FriendlyName;

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			_coordinatorChannel = AddressRegistry.GetOutboundCoordinatorChannel();

			Create();


//			_coordinatorChannel.Send(new ServiceCreated(_serviceName, _channel.Address, _channel.PipeName));
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

				object cfg = FastActivator.Create(typeof(ServiceConfigurator<>), new[] {serviceType});

				this.FastInvoke(new[] {serviceType}, "InitializeAndCreateHostedService", bootstrapper, cfg);
			}
			catch (Exception ex)
			{
				SendFault(ex);
			}
		}


// ReSharper disable UnusedMember.Local
		void InitializeAndCreateHostedService<T>(Bootstrapper<T> bootstrapper, ServiceConfigurator<T> cfg)
// ReSharper restore UnusedMember.Local
			where T : class
		{
			bootstrapper.FastInvoke("InitializeHostedService", cfg);

			_log.DebugFormat("[{0}] Creating service type: {1}", _serviceName, typeof(T).ToShortTypeName());


			_channel = AddressRegistry.GetInboundServiceChannel(AppDomain.CurrentDomain, x =>
				{
					x.AddConsumerOf<ServiceCreated>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServiceRunning>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServicePausing>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServicePaused>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServiceContinuing>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServiceStopping>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServiceStopped>()
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
					x.AddConsumerOf<ServiceUnloaded>() // TODO might want to make this unload the entire app domain
						.UsingConsumer(m => _coordinatorChannel.Send(m))
						.HandleOnCallingThread();
				});

			_service = cfg.Create(AppDomain.CurrentDomain.FriendlyName, null, _channel);

			var serviceCache = new Cache<string, ServiceStateMachine>();

			_channel.Connect(x =>
				{
					x.AddConsumersFor<ServiceStateMachine>()
						.BindUsing<ServiceStateMachineBinding, string>()
						.CreateNewInstanceBy(GetServiceInstance)
						.HandleOnInstanceFiber()
						.PersistInMemoryUsing(serviceCache);
				});

			_channel.Send(new CreateService(_serviceName, _channel.Address, _channel.PipeName));
		}
			
		ServiceStateMachine GetServiceInstance(string key)
		{
			if (key == null)
				return new ServiceStateMachine(null, _channel);

			if (key == _serviceName)
				return _service;

			throw new InvalidOperationException("An unknown service was requested: " + key);
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

			if (e.IsTerminating && _coordinatorChannel != null)
			{
				_coordinatorChannel.Send(new ServiceFault(_serviceName, e.ExceptionObject as Exception));
			}
		}

		void SendFault(Exception exception)
		{
			try
			{
				_coordinatorChannel.Send(new ServiceFault(_serviceName, exception));
			}
			catch (Exception)
			{
				_log.Error("[{0}] Failed to send fault".FormatWith(_serviceName), exception);
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