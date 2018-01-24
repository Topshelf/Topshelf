// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using HostConfigurators;
    using Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides Topshelf extensions for Microsoft extensions for logging.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Configures Topshelf to use Microsoft extensions for logging, using a <see cref="ILoggerFactory" /> instance to derive its loggers.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public static void UseLoggingExtensions(this HostConfigurator configurator, ILoggerFactory loggerFactory)
        {
            HostLogger.UseLogger(new LoggingExtensionsLogWriterFactory.LoggingExtensionsHostLoggerConfigurator(loggerFactory));
        }
    }
}