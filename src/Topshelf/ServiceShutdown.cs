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
    /// Implemented by services that support service shutdown
    /// </summary>
    public interface ServiceShutdown
    {
        /// <summary>
        /// Called when the operating system invokes the service shutdown method. There is little
        /// time to react here, but the application try to use RequestAdditionalTime if necessary,
        /// but this is really a shut down quick and bail method.
        /// </summary>
        /// <param name="hostControl"></param>
        void Shutdown(HostControl hostControl);
    }
}