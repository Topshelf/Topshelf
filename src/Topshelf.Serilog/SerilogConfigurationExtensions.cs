﻿// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using Serilog;

    public static class SerilogConfigurationExtensions
    {
        /// <summary>
        /// Configures Topshelf to use Serilog for logging, using the global <see cref="Log.Logger"/> instance to derive its loggers.
        /// </summary>
        public static void UseSerilog(this HostConfigurator configurator)
        {
            SerilogLogWriterFactory.Use(Log.Logger);
        }

        /// <summary>
        /// Configures Topshelf to use Serilog for logging, using the given <see cref="LoggerConfiguration"/> to create loggers.
        /// </summary>
        public static void UseSerilog(this HostConfigurator configurator, LoggerConfiguration loggerConfiguration)
        {
            SerilogLogWriterFactory.Use(loggerConfiguration.CreateLogger());
        }

        /// <summary>
        /// Configures Topshelf to use Serilog for logging, using the given root logger <see cref="ILogger"/> to create loggers.
        /// </summary>
        public static void UseSerilog(this HostConfigurator configurator, ILogger rootLogger)
        {
            SerilogLogWriterFactory.Use(rootLogger);
        }
    }
}
