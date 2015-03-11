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
namespace Topshelf.Logging
{
    using System;
    using Serilog;

    public class SerilogLogWriterFactory : LogWriterFactory
    {
        readonly Func<string, ILogger> _loggerFactory;

        SerilogLogWriterFactory(LoggerConfiguration loggerConfiguration)
        {
            _loggerFactory = name => loggerConfiguration.CreateLogger().ForContext("SourceContext", name);
        }

        SerilogLogWriterFactory()
        {
            _loggerFactory = name => Log.ForContext("SourceContext", name);
        }

        public LogWriter Get(string name)
        {
            return new SerilogLogWriter(_loggerFactory(name));
        }

        public void Shutdown()
        {
        }

        public static void Use()
        {
            HostLogger.UseLogger(new SerilogHostLoggerConfigurator());
        }

        public static void Use(LoggerConfiguration loggerConfiguration)
        {
            HostLogger.UseLogger(new SerilogHostLoggerConfigurator(loggerConfiguration));
        }

        [Serializable]
        public class SerilogHostLoggerConfigurator : HostLoggerConfigurator
        {
            readonly LoggerConfiguration _factory;

            public SerilogHostLoggerConfigurator(LoggerConfiguration factory)
            {
                _factory = factory;
            }

            public SerilogHostLoggerConfigurator()
            {
            }

            public LogWriterFactory CreateLogWriterFactory()
            {
                return _factory == null 
                    ? new SerilogLogWriterFactory() 
                    : new SerilogLogWriterFactory(_factory);
            }
        }
    }
}