// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    /// <summary>
    /// Defines a service recovery configurator.
    /// </summary>
    public interface ServiceRecoveryConfigurator
    {
        /// <summary>
        ///   Restart the service after waiting the delay period specified
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator RestartService(System.TimeSpan delay);

        /// <summary>
        ///   Restart the service after waiting the delay period in minutes
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator RestartService(int delayInMinutes);

        /// <summary>
        ///   Restart the computer after waiting the delay period specified
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="message"></param>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator RestartComputer(System.TimeSpan delay, string message);

        /// <summary>
        ///   Restart the computer after waiting the delay period in minutes
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <param name="message"> </param>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message);

        /// <summary>
        ///   Run the command specified
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="command">The command to run.</param>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator RunProgram(System.TimeSpan delay, string command);

        /// <summary>
        ///   Run the command specified
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <param name="command">The command to run.</param>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command);

        /// <summary>
        ///   Take no action
        /// </summary>
        /// <returns>The service recovery configurator.</returns>
        ServiceRecoveryConfigurator TakeNoAction();

        /// <summary>
        ///   Specifies the reset period for the restart options
        /// </summary>
        /// <param name="days">The reset period in days.</param>
        ServiceRecoveryConfigurator SetResetPeriod(int days);

        /// <summary>
        /// Specifies that the recovery actions should only be taken on a service crash. If the service exits
        /// with a non-zero exit code, it will not be restarted.
        /// </summary>
        void OnCrashOnly();
    }
}
