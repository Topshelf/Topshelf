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
namespace Topshelf.Specs.Config
{
	using Magnum.TestFramework;
	using NUnit.Framework;


	[Scenario]
	public class Given_an_empty_runner_configuration
	{
		[Then]
        [Ignore("Failing")]
		public void Should_throw_an_exception_when_executed()
		{
			Assert.Throws<HostConfigurationException>(() =>
				{
					HostFactory.Run(x =>
						{
							//
						});
				});
		}
	}

	[Scenario]
	public class Given_an_empty_runner_configuration_with_options_specified
	{
        [Ignore("Failing")]
        [Then]
		public void Should_throw_an_exception_when_executed()
		{
			Assert.Throws<HostConfigurationException>(() =>
				{
					HostFactory.New(x =>
						{
							x.SetDisplayName("My Service");
							x.SetServiceName("myservice");
							x.SetDescription("My service is most excellent.");

							x.Disabled();
							x.StartAutomatically();
							x.StartManually();

							x.RunAsNetworkService();
							x.RunAsLocalService();
							x.RunAsLocalSystem();
							x.RunAs("chris", "easy");

							x.BeforeInstall(() => { });
							x.AfterInstall(() => { });

							x.BeforeUninstall(() => { });
							x.AfterUninstall(() => { });
						}).Run();
				});
		}
	}

	[Scenario]
	public class Given_an_empty_runner_configuration_with_command_line_options
	{
		[Then]
        [Ignore("Failing")]
		public void Should_throw_an_exception_when_executed()
		{
			Assert.Throws<HostConfigurationException>(() =>
				{
					HostFactory.New(x =>
						{
							x.SetDisplayName("My Service");
							x.SetServiceName("myservice");
							x.SetDescription("My service is most excellent.");

							x.ApplyCommandLine(@"install -username:chris -password:easy -instance:first");

						}).Run();
				});
		}
	}
}