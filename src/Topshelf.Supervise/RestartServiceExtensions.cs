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

    public static class RestartServiceExtensions
    {
        /// <summary>
        ///   Schedules the service to restart at the specific interval. Useful for services that have known issues and need to be cycled regularly.
        /// </summary>
        /// <param name="configurator"> </param>
        /// <param name="interval"> The interval between restarts </param>
        public static void RestartEvery(this SuperviseConfigurator configurator, TimeSpan interval)
        {
        }
    }
}