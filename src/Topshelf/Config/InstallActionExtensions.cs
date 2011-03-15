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
	using Internal;
	using Model;


	public static class InstallActionExtensions
	{
		public static HostConfigurator BeforeInstall([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new InstallHostConfiguratorAction(x => x.BeforeInstall(callback)));

			return configurator;
		}

		public static HostConfigurator AfterInstall([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new InstallHostConfiguratorAction(x => x.AfterInstall(callback)));

			return configurator;
		}

		public static HostConfigurator BeforeUninstall([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new UninstallHostConfiguratorAction(x => x.BeforeUninstall(callback)));

			return configurator;
		}

		public static HostConfigurator AfterUninstall([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new UninstallHostConfiguratorAction(x => x.AfterUninstall(callback)));

			return configurator;
		}

		public static HostConfigurator BeforeStartingServices([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.BeforeStartingServices(_ => callback())));

			return configurator;
		}

		public static HostConfigurator BeforeStartingServices([NotNull] this HostConfigurator configurator,
		                                                      Action<IServiceCoordinator> callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.BeforeStartingServices(callback)));

			return configurator;
		}

		public static HostConfigurator AfterStartingServices([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStartingServices(_ => callback())));

			return configurator;
		}

		public static HostConfigurator AfterStartingServices([NotNull] this HostConfigurator configurator,
		                                                     Action<IServiceCoordinator> callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStartingServices(callback)));

			return configurator;
		}

		public static HostConfigurator AfterStoppingServices([NotNull] this HostConfigurator configurator, Action callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStoppingServices(_ => callback())));

			return configurator;
		}

		public static HostConfigurator AfterStoppingServices([NotNull] this HostConfigurator configurator,
		                                                     Action<IServiceCoordinator> callback)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.AfterStoppingServices(callback)));

			return configurator;
		}

		public static HostConfigurator SetEventTimeout([NotNull] this HostConfigurator configurator, TimeSpan timeout)
		{
			if (configurator == null)
				throw new ArgumentNullException("configurator");

			configurator.AddConfigurator(new RunHostConfiguratorAction(x => x.SetEventTimeout(timeout)));

			return configurator;
		}
	}
}