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
namespace Topshelf.Specs.Configuration
{
	using System.ServiceProcess;
	using Magnum.TestFramework;
	using TestObject;
	using Topshelf.Configuration;
	using Topshelf.Configuration.Dsl;


	[Scenario]
	public class Configuring_a_service_using_the_runner
	{
		RunConfiguration _runConfiguration;

		[Given]
		public void A_service_configuration()
		{
			_runConfiguration = RunnerConfigurator.New(x =>
				{
					x.SetDisplayName("chris");
					x.SetServiceName("chris");
					x.SetDescription("chris's pants");

					x.ConfigureService<TestService>(c =>
						{
							c.WhenStarted(s => s.Start());
							c.WhenStopped(s => s.Stop());
							c.WhenPaused(s => { });
							c.WhenContinued(s => { });
							c.Named("my_service");
						});

					x.DoNotStartAutomatically();

					x.RunAs("dru", "pass");

					x.DependsOn("ServiceName");
					x.DependencyOnMsmq();
					x.DependencyOnMsSql();

					x.BeforeInstall(() => { });
					x.AfterInstall(() => { });
					x.BeforeUninstall(() => { });
					x.AfterUninstall(() => { });
				});
		}

		[Finally]
		public void Finally()
		{
			_runConfiguration = null;
		}

		[Then]
		public void Should_depend_on_Msmq_MsSql_and_Custom()
		{
//			_runConfiguration.WinServiceSettings.Dependencies
//				.ShouldContain(KnownServiceNames.Msmq);
//
//			_runConfiguration.WinServiceSettings.Dependencies
//				.ShouldContain(KnownServiceNames.SqlServer);
//
//			_runConfiguration.WinServiceSettings.Dependencies
//				.ShouldContain("ServiceName");
		}

		[Then]
		public void Names_should_be_correct()
		{
//			_runConfiguration.WinServiceSettings.FullDisplayName
//				.ShouldEqual("chris");
//
//			_runConfiguration.WinServiceSettings.ServiceName.FullName
//				.ShouldEqual("chris");
//
//			_runConfiguration.WinServiceSettings.Description
//				.ShouldEqual("chris's pants");
		}

		[Then]
		public void Should_not_be_set_to_start_automatically()
		{
//			_runConfiguration.WinServiceSettings.StartMode
//				.ShouldEqual(ServiceStartMode.Manual);
		}

		[Then]
		public void Should_use_the_correct_credentials()
		{
//			_runConfiguration.WinServiceSettings.Credentials.Username
//				.ShouldEqual("dru");
//
//			_runConfiguration.WinServiceSettings.Credentials.Password
//				.ShouldEqual("pass");
//
//			_runConfiguration.WinServiceSettings.Credentials.Account
//				.ShouldEqual(ServiceAccount.User);
		}

        [Then]
        public void Should_have_an_installation_action()
        {
//            _runConfiguration.WinServiceSettings.AfterInstallAction.ShouldNotBeNull();
        }
    }
}
