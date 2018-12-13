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
    /// Represents a take no action recovery action.
    /// </summary>
    public class TakeNoActionAction :
        ServiceRecoveryAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeNoActionAction"/> class.
        /// </summary>
        public TakeNoActionAction()
            : base(TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Gets the service recovery configuration action.
        /// </summary>
        /// <returns>A <see cref="NativeMethods.SC_ACTION"/> representing the take no action service recovery configuration action.</returns>
        public override NativeMethods.SC_ACTION GetAction()
        {
            return new NativeMethods.SC_ACTION
            {
                Delay = Delay,
                Type = (int)NativeMethods.SC_ACTION_TYPE.None
            };
        }
    }
}