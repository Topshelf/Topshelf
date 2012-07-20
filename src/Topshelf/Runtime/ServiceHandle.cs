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
namespace Topshelf.Runtime
{
    using System;


    /// <summary>
    /// A handle to a service being hosted by the Host
    /// </summary>
    public interface ServiceHandle :
        IDisposable
    {
        bool Start(HostControl hostControl);



        bool Pause(HostControl hostControl);
       
        bool Continue(HostControl hostControl);


        /// <summary>
        /// Stop the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was stopped, or false if the service cannot be stopped at this time</returns>
        bool Stop(HostControl hostControl);
    }
}