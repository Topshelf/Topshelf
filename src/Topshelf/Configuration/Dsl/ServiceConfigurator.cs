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
namespace Topshelf.Configuration.Dsl
{
	using System;
	using Magnum.Channels;
	using Magnum.Reflection;
	using Model;


	public class ServiceConfigurator<TService> :
		IServiceConfigurator<TService>
		where TService : class
	{
		bool _disposed;

		public ServiceConfigurator()
		{
			PauseAction = DoNothing;
			StartAction = DoNothing;
			StopAction = DoNothing;
			ContinueAction = DoNothing;

			BuildAction = (name, coordinator) => FastActivator<TService>.Create();

			Name = "{0}/{1}".FormatWith(typeof(TService).Name, Guid.NewGuid());
		}

		public string Name { get; private set; }

		Action<TService> ContinueAction { get; set; }
		Action<TService> PauseAction { get; set; }
		Action<TService> StartAction { get; set; }
		Action<TService> StopAction { get; set; }

		InternalServiceFactory<TService> BuildAction { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Named(string name)
		{
			Name = name;
		}

		public void WhenStarted(Action<TService> startAction)
		{
			StartAction = startAction;
		}

		public void WhenStopped(Action<TService> stopAction)
		{
			StopAction = stopAction;
		}

		public void WhenPaused(Action<TService> pauseAction)
		{
			PauseAction = pauseAction;
		}

		public void WhenContinued(Action<TService> continueAction)
		{
			ContinueAction = continueAction;
		}

		public void HowToBuildService(ServiceFactory<TService> factory)
		{
			BuildAction = (name, coordinator) => factory(name);
		}

		public void ConstructUsing(ServiceFactory<TService> factory)
		{
			BuildAction = (name,coordinator) => factory(name);
		}

		public void ConstructUsing(InternalServiceFactory<TService> serviceFactory)
		{
			BuildAction = serviceFactory;
		}

		public ServiceController<TService> Create(IServiceCoordinator coordinator, ServiceChannel coordinatorChannel)
		{
			return Create(Name, coordinator, coordinatorChannel);
		}

		public ServiceController<TService> Create(string serviceName, IServiceCoordinator coordinator, ServiceChannel coordinatorChannel)
		{
			var serviceController = new ServiceController<TService>(serviceName, coordinator, coordinatorChannel,
			                                                        StartAction,
			                                                        StopAction,
			                                                        PauseAction,
			                                                        ContinueAction,
			                                                        BuildAction);

			return serviceController;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;
			if (_disposed)
				return;

			StartAction = null;
			StopAction = null;
			PauseAction = null;
			ContinueAction = null;
			BuildAction = null;

			_disposed = true;
		}

		~ServiceConfigurator()
		{
			Dispose(false);
		}

		static void DoNothing(TService instance)
		{
		}
	}
}