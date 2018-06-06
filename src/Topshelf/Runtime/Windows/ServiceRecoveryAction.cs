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
    using System;

    /// <summary>
    /// Represents a service recovery action.
    /// </summary>
    public abstract class ServiceRecoveryAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRecoveryAction"/> class.
        /// </summary>
        /// <param name="delay">The delay.</param>
        protected ServiceRecoveryAction(TimeSpan delay)
        {
            Delay = (int)delay.TotalMilliseconds;
        }

        /// <summary>
        /// Gets the delay in milliseconds.
        /// </summary>
        /// <value>The delay in milliseconds.</value>
        public int Delay { get; }

        /// <summary>
        /// Gets the service recovery configuration action.
        /// </summary>
        /// <returns>A <see cref="NativeMethods.SC_ACTION"/> representing the service recovery configuration action.</returns>
        public abstract NativeMethods.SC_ACTION GetAction();
    }
}