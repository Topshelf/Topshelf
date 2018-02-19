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

    /// <summary>
    /// Represents an option to set a service start timeout (in seconds).
    /// </summary>
    /// <seealso cref="Option" />
    internal class StartTimeoutOption
        : Option
    {
        /// <summary>
        /// The start timeout (in seconds).
        /// </summary>
        private readonly int startTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartTimeoutOption"/> class.
        /// </summary>
        /// <param name="startTimeout">The start timeout (in seconds).</param>
        public StartTimeoutOption(int startTimeout)
        {
            this.startTimeout = startTimeout;
        }

        /// <summary>
        /// Applies the option to the specified host configurator.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetStartTimeout(System.TimeSpan.FromSeconds(this.startTimeout));
        }
    }
}
