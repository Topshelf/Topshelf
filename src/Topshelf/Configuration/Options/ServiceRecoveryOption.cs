// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Options
{
    using Topshelf.HostConfigurators;
    using Topshelf.Runtime.Windows;

    /// <summary>
    /// Represents an option to set a service recovery options.
    /// </summary>
    /// <seealso cref="Option" />
    public class ServiceRecoveryOption
        : Option
    {
        private readonly ServiceRecoveryOptions serviceRecoveryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRecoveryOption"/> class.
        /// </summary>
        /// <param name="serviceRecoveryOptions">The service recovery options.</param>
        public ServiceRecoveryOption(ServiceRecoveryOptions serviceRecoveryOptions)
        {
            this.serviceRecoveryOptions = serviceRecoveryOptions;
        }

        /// <summary>
        /// Applies the option to the specified host configurator.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        public void ApplyTo(HostConfigurator configurator)
        {
            var recoveryHostConfigurator = new ServiceRecoveryHostConfigurator();

            foreach (var option in serviceRecoveryOptions.Actions)
            {
                switch (option)
                {
                    case RestartServiceRecoveryAction restartServiceRecoveryAction:
                        recoveryHostConfigurator.RestartService(restartServiceRecoveryAction.Delay / 60000);
                        break;
                    case RestartSystemRecoveryAction restartSystemRecoveryAction:
                        recoveryHostConfigurator.RestartComputer(restartSystemRecoveryAction.Delay / 60000, restartSystemRecoveryAction.RestartMessage);
                        break;
                    case RunProgramRecoveryAction runProgramRecoveryAction:
                        recoveryHostConfigurator.RunProgram(runProgramRecoveryAction.Delay / 60000, runProgramRecoveryAction.Command);
                        break;
                }
            }

            if (this.serviceRecoveryOptions.RecoverOnCrashOnly)
            {
                recoveryHostConfigurator.OnCrashOnly();
            }

            recoveryHostConfigurator.SetResetPeriod(this.serviceRecoveryOptions.ResetPeriod);

            configurator.AddConfigurator(recoveryHostConfigurator);
        }
    }
}
