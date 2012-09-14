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
    using Builders;
    using HostConfigurators;

    public static class TestHostExtensions
    {
        /// <summary>
        /// Configures the test host, which simply starts and stops the service. Meant to be used
        /// to verify the service can be created, started, stopped, and disposed without issues.
        /// </summary>
        public static HostConfigurator UseTestHost(this HostConfigurator configurator)
        {
            configurator.UseHostBuilder((environment, settings) => new TestBuilder(environment, settings));
            configurator.ApplyCommandLine("");

            return configurator;
        }
    }
}