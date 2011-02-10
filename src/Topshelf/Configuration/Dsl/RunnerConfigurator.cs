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
	using Dashboard;
	using HostConfigurators;


	[Obsolete("Use HostFactory instead")]
	public class RunnerConfigurator :
		HostConfiguratorImpl,
		IRunnerConfigurator
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "RunnerConfigurator" /> class.
		/// </summary>
		RunnerConfigurator()
		{
			SetServiceName("Topshelf Service");

			this.RunAsLocalSystem();
		}

		public void DoNotStartAutomatically()
		{
			this.StartManually();
		}

		public void EnableDashboard()
		{
			ConfigureService<TopshelfDashboard>(o =>
			{
				o.ConstructUsing((name, coordinator) => new TopshelfDashboard(Description, coordinator));
				o.WhenStarted(s => s.Start());
				o.WhenStopped(s => s.Stop());
			});
		}

		public void ConfigureService<TService>(Action<IServiceConfigurator<TService>> action)
			where TService : class
		{
			this.Service<TService>(x =>
				{
					var configurator = new ServiceConfiguratorImpl<TService>(x);

					action(configurator);
				});
		}

		public void UseServiceRecovery(Action<IServiceRecoveryConfigurator> recoveryConfigurator)
		{
			//recoveryConfigurator(new ServiceRecoveryConfigurator(_winServiceSettings.ServiceRecoveryOptions));
		}

		public void RunAsFromInteractive()
		{
			this.RunAs("", "");
		}

		public void RunAsFromCommandLine()
		{
		}

		public static RunConfiguration New(Action<IRunnerConfigurator> action)
		{
			var configurator = new RunnerConfigurator();

			action(configurator);

			return configurator.Create();
		}

		RunConfiguration Create()
		{
			return new RunConfiguration(CreateHost());
		}
	}
}