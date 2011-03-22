// Copyright 2007-2011 The Apache Software Foundation.
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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using Builders;
	using Configuration.Dsl;
	using log4net;
	using log4net.Config;
	using Magnum.Extensions;
	using Magnum.Reflection;
	using Messages;
	using Shelving;
	using Stact;
	using Stact.Configuration;


	[DebuggerDisplay("Shelf[{ServiceName}]")]
	public class Shelf :
		IDisposable
	{
		readonly Type _bootstrapperType;
		readonly Uri _controllerAddress;
		readonly string _controllerPipeName;
		readonly ILog _log;
		readonly string _serviceName;
		HostChannel _channel;
		OutboundChannel _controllerChannel;
		bool _disposed;
		PoolFiber _fiber;
		Host _host;

		public Shelf(Type bootstrapperType, Uri controllerAddress, string controllerPipeName)
		{
			_bootstrapperType = bootstrapperType;
			_controllerAddress = controllerAddress;
			_controllerPipeName = controllerPipeName;

			BootstrapLogger();

			_serviceName = AppDomain.CurrentDomain.FriendlyName;

			_log = LogManager.GetLogger("Topshelf.Shelf." + _serviceName);

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
			AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;

			Create();
		}

		void OnDomainUnload(object sender, EventArgs e)
		{
			LogManager.Shutdown();
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
				if (_channel != null)
				{
					_channel.Dispose();
					_channel = null;
				}

				if (_controllerChannel != null)
				{
					_controllerChannel.Dispose();
					_controllerChannel = null;
				}

				LogManager.Shutdown();
			}

			_disposed = true;
		}

		void Create()
		{
			try
			{
				_controllerChannel = new OutboundChannel(_controllerAddress, _controllerPipeName);

				Type type = FindBootstrapperImplementationType(_bootstrapperType);

				_log.DebugFormat("[{0}] Creating bootstrapper: {1}", _serviceName, type.ToShortTypeName());

				object bootstrapper = FastActivator.Create(type);

				Type serviceType = bootstrapper.GetType()
					.GetInterfaces()
					.First()
					.GetGenericArguments()
					.First();

				_log.DebugFormat("[{0}] Creating configurator for service type: {1}", _serviceName, serviceType.ToShortTypeName());

				InitializeAndCreateService(serviceType, bootstrapper);
			}
			catch (Exception ex)
			{
				SendFault(ex);
			}
		}

		void InitializeAndCreateService(Type serviceType, object bootstrapper)
		{
			this.FastInvoke(new[] {serviceType}, "InitializeAndCreateHostedService", bootstrapper);
		}

		// ReSharper disable UnusedMember.Local
		void InitializeAndCreateHostedService<T>(Bootstrapper<T> bootstrapper)
			// ReSharper restore UnusedMember.Local
			where T : class
		{
			_log.DebugFormat("[{0}] Creating service type: {1}", _serviceName, typeof(T).ToShortTypeName());

			_fiber = new PoolFiber();

			_channel = HostChannelFactory.CreateShelfHost(_serviceName, AddEventForwarders);

			_controllerChannel.Send(new ShelfCreated(_serviceName, _channel.Address, _channel.PipeName));

			_host = HostFactory.New(x =>
				{
					x.SetServiceName(_serviceName);
					x.UseBuilder(description => new ShelfBuilder(description, _channel));

					x.Service<T>(s =>
						{
							var serviceConfigurator = new ServiceConfiguratorImpl<T>(s);

							bootstrapper.InitializeHostedService(serviceConfigurator);

							s.SetServiceName(_serviceName);
						});
				});

			_host.Run();
		}

		void AddEventForwarders(ConnectionConfigurator x)
		{
			// These are needed to ensure that events are updated in the shelf state machine
			// as well as the service coordinator state machine. To handle this, the servicecontroller
			// is given the shelf channel as the reporting channel for events, and the shelf forwards
			// the events to the service coordinator

			x.AddConsumerOf<ServiceEvent>()
				.UsingConsumer(OnServiceEvent)
				.HandleOnCallingThread();

			x.AddConsumerOf<ServiceCreated>()
				.UsingConsumer(m => _controllerChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceFolderChanged>()
				.UsingConsumer(m => _controllerChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceFolderRemoved>()
				.UsingConsumer(m => _controllerChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceRunning>()
				.UsingConsumer(m => _controllerChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceStopped>()
				.UsingConsumer(m => _controllerChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServicePaused>()
				.UsingConsumer(m => _controllerChannel.Send(m))
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceUnloaded>()
				.UsingConsumer(m =>
					{
						_log.InfoFormat("[{0}] Unloading Shelf and AppDomain", _serviceName);
						_controllerChannel.Send(m);
						Dispose();
						AppDomain.Unload(AppDomain.CurrentDomain);
					})
				.HandleOnFiber(_fiber);
			x.AddConsumerOf<ServiceFault>()
				.UsingConsumer(m => _controllerChannel.Send(m))
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
			_log.ErrorFormat("Unhandled {0}exception in app domain {1}: {2}", e.IsTerminating ? "terminal " : "",
			                 AppDomain.CurrentDomain.FriendlyName,
			                 e.ExceptionObject);

			Dispose();
		}

		void SendFault(Exception ex)
		{
			try
			{
				_controllerChannel.Send(new ServiceFault(_serviceName, ex));
			}
			catch (Exception)
			{
				_log.Error("[{0}] Failed to send fault".FormatWith(_serviceName), ex);
			}
		}

		void OnServiceEvent(ServiceEvent message)
		{
			_log.InfoFormat("<{0}> {1}", message.ServiceName, message.EventType);
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