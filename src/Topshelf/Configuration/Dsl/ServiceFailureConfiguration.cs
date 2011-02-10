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
	using Windows;
	using WindowsServiceCode;


	public class ServiceFailureConfiguration :
		IServiceFailureConfiguration
	{
		readonly ServiceFailure _serviceFailure;
		readonly ServiceRecoveryOptions _serviceRecoveryOptions;

		public ServiceFailureConfiguration(ServiceRecoveryOptions serviceRecoveryOptions, ServiceFailure serviceFailure)
		{
			_serviceRecoveryOptions = serviceRecoveryOptions;
			_serviceFailure = serviceFailure;
		}

		public void RestartService()
		{
			SetRecoveryOptionForFailure(ServiceRecoveryAction.RestartService);
		}

		public IServiceFailureRestartConfiguration RestartComputer()
		{
			SetRecoveryOptionForFailure(ServiceRecoveryAction.RestartComputer);

			return new ServiceFailureRestartConfiguration(_serviceRecoveryOptions);
		}

		void SetRecoveryOptionForFailure(ServiceRecoveryAction recoveryAction)
		{
			switch (_serviceFailure)
			{
				case ServiceFailure.First:
					_serviceRecoveryOptions.FirstFailureAction = recoveryAction;
					break;
				case ServiceFailure.Second:
					_serviceRecoveryOptions.SecondFailureAction = recoveryAction;
					break;
				case ServiceFailure.Subsequent:
					_serviceRecoveryOptions.SubsequentFailureAction = recoveryAction;
					break;
			}
		}
	}
}