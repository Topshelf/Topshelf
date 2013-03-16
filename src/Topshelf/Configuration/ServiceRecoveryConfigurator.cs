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
    public interface ServiceRecoveryConfigurator
    {
        /// <summary>
        ///   Restart the service after waiting the delay period specified
        /// </summary>
        /// <param name="delayInMinutes"> </param>
        ServiceRecoveryConfigurator RestartService(int delayInMinutes);

        /// <summary>
        ///   Restart the computer after waiting the delay period in minutes
        /// </summary>
        /// <param name="delayInMinutes"> </param>
        /// <param name="message"> </param>
        ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message);

        /// <summary>
        ///   Run the command specified
        /// </summary>
        /// <param name="delayInMinutes"> </param>
        /// <param name="command"> </param>
        ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command);

        /// <summary>
        ///   Specifies the reset period for the restart options
        /// </summary>
        /// <param name="days"> </param>
        ServiceRecoveryConfigurator SetResetPeriod(int days);

        /// <summary>
        /// Specifies that the recovery actions should only be taken on a service crash. If the service exists
        /// with a non-zero exit code, it will not be restarted.
        /// </summary>
        void OnCrashOnly();
    }
}