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
namespace Topshelf.Configuration.Dsl
{
	using System;
	using Extensions;
	using Magnum.Reflection;
	using Model;
	using ServiceConfigurators;


	[Obsolete("Proxy in place for legacy configuration DSL")]
	public class ServiceConfiguratorImpl<TService> :
		IServiceConfigurator<TService>
		where TService : class
	{
		readonly ServiceConfigurator<TService> _configurator;

		public ServiceConfiguratorImpl(ServiceConfigurator<TService> configurator)
		{
			_configurator = configurator;

			_configurator.ConstructUsing((name, coordinator) => FastActivator<TService>.Create());

			_configurator.SetServiceName("{0}/{1}".FormatWith(typeof(TService).Name, Guid.NewGuid()));
		}

		public void Named(string name)
		{
			_configurator.SetServiceName(name);
		}

		public void HowToBuildService(ServiceFactory<TService> factory)
		{
			_configurator.ConstructUsing((name, coordinator) => factory(name));
		}

		public void ConstructUsing(ServiceFactory<TService> factory)
		{
			_configurator.ConstructUsing((name, coordinator) => factory(name));
		}

		public void Validate()
		{
			_configurator.Validate();
		}

		public void SetServiceName(string name)
		{
			_configurator.SetServiceName(name);
		}

		public void ConstructUsing(InternalServiceFactory<TService> serviceFactory)
		{
			_configurator.ConstructUsing(serviceFactory);
		}

		public void WhenStarted(Action<TService> startAction)
		{
			_configurator.WhenStarted(startAction);
		}

		public void WhenStopped(Action<TService> stopAction)
		{
			_configurator.WhenStopped(stopAction);
		}

		public void WhenPaused(Action<TService> pauseAction)
		{
			_configurator.WhenPaused(pauseAction);
		}

		public void WhenContinued(Action<TService> continueAction)
		{
			_configurator.WhenContinued(continueAction);
		}
	}
}