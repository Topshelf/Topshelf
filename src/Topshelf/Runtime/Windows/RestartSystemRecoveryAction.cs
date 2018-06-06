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
namespace Topshelf.Runtime.Windows
{
    /// <summary>
    /// Represents a restart system service recovery action.
    /// </summary>
    /// <seealso cref="Topshelf.Runtime.Windows.ServiceRecoveryAction" />
    public class RestartSystemRecoveryAction :
        ServiceRecoveryAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestartSystemRecoveryAction"/> class.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="restartMessage">The restart message.</param>
        public RestartSystemRecoveryAction(System.TimeSpan delay, string restartMessage)
            : base(delay)
        {
            RestartMessage = restartMessage;
        }

        /// <summary>
        /// Gets the system restart message.
        /// </summary>
        /// <value>The system restart message.</value>
        public string RestartMessage { get; private set; }

        /// <summary>
        /// Gets the service recovery configuration action.
        /// </summary>
        /// <returns>A <see cref="NativeMethods.SC_ACTION"/> representing the restart system service recovery configuration action.</returns>
        public override NativeMethods.SC_ACTION GetAction()
        {
            return new NativeMethods.SC_ACTION
            {
                Delay = Delay,
                Type = (int)NativeMethods.SC_ACTION_TYPE.RebootComputer,
            };
        }
    }
}