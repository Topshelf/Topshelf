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
    using System;

    /// <summary>
    /// Allows the service to control the host while running
    /// </summary>
    public interface HostControl
    {
        /// <summary>
        /// Tells the Host that the service is still starting, which resets the
        /// timeout.
        /// </summary>
        void RequestAdditionalTime(TimeSpan timeRemaining);

        /// <summary>
        /// Stops the Host
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the Host, returning the specified exit code
        /// </summary>
        void Stop(TopshelfExitCode exitCode);

        /// <summary>
        /// Restarts the Host
        /// </summary>
        void Restart();
    }
}