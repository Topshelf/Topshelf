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
	public class RestartComputerConfiguratorImpl :
		RestartComputerConfigurator
	{
		readonly RecoveryHostConfigurator _configurator;

		public RestartComputerConfiguratorImpl(RecoveryHostConfigurator configurator)
		{
			_configurator = configurator;
		}

		public RestartComputerConfigurator DisplayMessage(string message)
		{
			_configurator.SetRestartSystemMessage(message);

			return this;
		}

		public RestartComputerConfigurator Wait(int minutes)
		{
			_configurator.SetRestartDelay(minutes);

			return this;
		}

		public FailureConfigurator OnFirstFailure()
		{
			return _configurator.OnFirstFailure();
		}

		public FailureConfigurator OnSecondFailure()
		{
			return _configurator.OnSecondFailure();
		}

		public FailureConfigurator OnSubsequentFailures()
		{
			return _configurator.OnSubsequentFailures();
		}

		public void ResetFailureCountAfter(int days)
		{
			_configurator.ResetFailureCountAfter(days);
		}
	}
}