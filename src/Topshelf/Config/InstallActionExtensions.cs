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
namespace Topshelf
{
	using System;
	using HostConfigurators;
	using Model;


	public static class InstallActionExtensions
	{
		public static HostConfigurator BeforeInstall(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new InstallHostConfiguratorAction(x => x.BeforeInstall(callback)));

			return configurator;
		}

		public static HostConfigurator AfterInstall(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new InstallHostConfiguratorAction(x => x.AfterInstall(callback)));

			return configurator;
		}

		public static HostConfigurator BeforeUninstall(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new UninstallHostConfiguratorAction(x => x.BeforeUninstall(callback)));

			return configurator;
		}

		public static HostConfigurator AfterUninstall(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new UninstallHostConfiguratorAction(x => x.AfterUninstall(callback)));

			return configurator;
		}

		public static HostConfigurator BeforeStartingServices(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.BeforeStartingServices(_ => callback())));

			return configurator;
		}

		public static HostConfigurator BeforeStartingServices(this HostConfigurator configurator, Action<IServiceCoordinator> callback)
		{
			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.BeforeStartingServices(callback)));

			return configurator;
		}

		public static HostConfigurator AfterStartingServices(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStartingServices(_ => callback())));

			return configurator;
		}

		public static HostConfigurator AfterStartingServices(this HostConfigurator configurator, Action<IServiceCoordinator> callback)
		{
			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStartingServices(callback)));

			return configurator;
		}

		public static HostConfigurator AfterStoppingServices(this HostConfigurator configurator, Action callback)
		{
			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStoppingServices(_ => callback())));

			return configurator;
		}

		public static HostConfigurator AfterStoppingServices(this HostConfigurator configurator, Action<IServiceCoordinator> callback)
		{
			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStoppingServices(callback)));

			return configurator;
		}
	}
}