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
namespace Topshelf.Logging
{
    using System;
    using System.IO;
    using log4net;
    using log4net.Config;

    public class Log4NetLogWriterFactory :
        LogWriterFactory
    {
        public LogWriter Get(string name)
        {
            return new Log4NetLogWriter(LogManager.GetLogger(name));
        }

        public void Shutdown()
        {
            LogManager.Shutdown();
        }

        public static void Use()
        {
            HostLogger.UseLogger(new Log4NetLoggerConfigurator(null));
        }

        public static void Use(string file)
        {
            HostLogger.UseLogger(new Log4NetLoggerConfigurator(file));
        }

        public static void Use(string file, bool watch)
        {
            HostLogger.UseLogger(new Log4NetLoggerConfigurator(file, watch));
        }

        [Serializable]
        public class Log4NetLoggerConfigurator :
            HostLoggerConfigurator
        {
            readonly string _file;
            readonly bool _watch;

            public Log4NetLoggerConfigurator(string file)
              : this(file, false)
            {
            }

            public Log4NetLoggerConfigurator(string file, bool watch)
            {
                _file = file;
                _watch = watch;
            }

            public LogWriterFactory CreateLogWriterFactory()
            {
                if (!string.IsNullOrEmpty(_file))
                {
                    string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _file);
                    var configFile = new FileInfo(file);
                    if (configFile.Exists)
                    {
                        if (_watch)
                        {
                            XmlConfigurator.ConfigureAndWatch(configFile);
                        }
                        else
                        {
                            XmlConfigurator.Configure(configFile);
                        }
                    }
                }

                return new Log4NetLogWriterFactory();
            }
        }
    }
}