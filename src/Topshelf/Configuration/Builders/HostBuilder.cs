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
namespace Topshelf.Builders
{
    using System;
    using Runtime;

    /// <summary>
    /// Using the service configuration, the host builder will create the host
    /// that will be ran by the service console.
    /// </summary>
    public interface HostBuilder
    {
        HostEnvironment Environment { get; }
        HostSettings Settings { get; }

        Host Build();

        void Match<T>(Action<T> callback)
            where T : class, HostBuilder;
    }
}