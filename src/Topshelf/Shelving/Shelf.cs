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
	using Magnum.Reflection;
	using Messages;
	using Model;


	[DebuggerDisplay("Shelf[{ServiceName}]")]
	public class Shelf :
		IDisposable
	{
		readonly Uri _address;
		readonly Type _bootstrapperType;
		readonly OutboundChannel _coordinatorChannel;
		readonly ILog _log;
		readonly string _pipeName;
		readonly string _serviceName;
		readonly InboundChannel _channel;
		IServiceController _service;
		InboundChannel _serviceChannel;


		public Shelf(Type bootstrapperType)
		{
			BootstrapLogger();

			_log = LogManager.GetLogger(typeof(Shelf));

			_serviceName = AppDomain.CurrentDomain.FriendlyName;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			_coordinatorChannel = AddressRegistry.GetOutboundCoordinatorChannel();
			_channel = AddressRegistry.GetInboundServiceChannel(AppDomain.CurrentDomain, x =>
			                                   	{
			                                   		x.AddConsumerOf<StartService>()
			                                   			.UsingConsumer(msg => Start());
			                                   	});


			_bootstrapperType = bootstrapperType;

			_coordinatorChannel.Send(new ServiceCreated(_serviceName, _address, _pipeName));
		}

		public void Dispose()
		{
			if (_channel != null)
				_channel.Dispose();
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_log.Error("Unhandled {0}exception in app domain {1}: {2}".FormatWith(e.IsTerminating ? "terminal " : "",
			                                                                      AppDomain.CurrentDomain.FriendlyName,
			                                                                      e.ExceptionObject));

			if (e.IsTerminating && _coordinatorChannel != null)
			{
				_coordinatorChannel.Send(new ShelfFault(e.ExceptionObject as Exception)
					{
						ServiceName = _serviceName
					});
			}
		}

		void Start()
		{
			try
			{
				Type t = FindBootstrapperImplementationType(_bootstrapperType);
				object bootstrapper = Activator.CreateInstance(t);

				Type serviceType = bootstrapper.GetType().GetInterfaces()[0].GetGenericArguments()[0];
				object cfg = FastActivator.Create(typeof(ServiceConfigurator<>).MakeGenericType(serviceType));

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

			ServiceController<T> service = cfg.Create(AppDomain.CurrentDomain.FriendlyName, _channel);

			_serviceChannel = new InboundChannel(AddressRegistry.GetShelfServiceInstanceAddress(AppDomain.CurrentDomain),
			                                     AddressRegistry.GetShelfServiceInstancePipeName(AppDomain.CurrentDomain),
			                                     x =>
			                                     	{
			                                     		x.AddConsumersFor<ServiceStateMachine>()
			                                     			.UsingInstance(service);
			                                     	});


			_serviceChannel.Send(new CreateService());

			_service = service;
		}

		public static Type FindBootstrapperImplementationType(Type bootstrapper)
		{
			if (bootstrapper != null)
			{
				if (bootstrapper.GetInterfaces().Where(IsBootstrapperType).Count() > 0)
					return bootstrapper;

				throw new InvalidOperationException(
					"Bootstrapper type, '{0}', is not a subclass of Bootstrapper.".FormatWith(bootstrapper.GetType().
					                                                                          	Name));
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

		void SendFault(Exception exception)
		{
			try
			{
				_coordinatorChannel.Send(new ShelfFault(exception));
			}
			catch (Exception)
			{
				// eat the exception for now
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