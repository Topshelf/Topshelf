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
namespace Topshelf.HostConfigurators
{
	using System;
	using Builders;
	using log4net;
	using Windows;


	public class RecoveryHostConfigurator :
		RecoveryConfigurator,
		HostBuilderConfigurator
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.HostConfigurators.RecoveryHostConfigurator");
		ServiceRecoveryOptions _options;

		public RecoveryHostConfigurator()
		{
			_options = new ServiceRecoveryOptions();
		}

		public void Validate()
		{
		}

		public void Configure(HostBuilder builder)
		{
			builder.Match<InstallBuilder>(x => x.AfterInstall(() => SetRecoveryOptions(x.Description)));
		}

		public FailureConfigurator OnFirstFailure()
		{
			var failureConfigurator = new FailureConfiguratorImpl(this, x => { _options.FirstFailureAction = x; });

			return failureConfigurator;
		}

		public FailureConfigurator OnSecondFailure()
		{
			var failureConfigurator = new FailureConfiguratorImpl(this, x => { _options.SecondFailureAction = x; });

			return failureConfigurator;
		}

		public FailureConfigurator OnSubsequentFailures()
		{
			var failureConfigurator = new FailureConfiguratorImpl(this, x => { _options.SubsequentFailureAction = x; });

			return failureConfigurator;
		}

		public void ResetFailureCountAfter(int days)
		{
			_options.ResetFailureCountWaitDays = days;
		}

		void SetRecoveryOptions(ServiceDescription description)
		{
			_log.DebugFormat("Setting service recovery options for {0}", description.GetServiceName());

			try
			{
				WindowsServiceControlManager.SetServiceRecoveryOptions(description.GetServiceName(), _options);
			}
			catch (Exception ex)
			{
				_log.Error("Failed to set service recovery options", ex);
			}
		}

		public void SetRestartDelay(int minutes)
		{
			_options.RestartServiceWaitMinutes = minutes;
		}

		public void SetComputerRestartDelay(int minutes)
		{
			_options.RestartSystemWaitMinutes = minutes;
		}

		public void SetRestartSystemMessage(string message)
		{
			_options.RestartSystemMessage = message;
		}

		public void SetProgram(string command)
		{
			_options.RunProgramCommand = command;
		}

		public void SetProgramParameters(string parameters)
		{
			_options.RunProgramParameters = parameters;
		}
	}
}