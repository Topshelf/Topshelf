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
    using Internals.Extensions;

    public static class HostLogger
    {
        static readonly object _locker = new object();
        static HostLoggerConfigurator _configurator;
        static LogWriterFactory _logWriterFactory;

        public static LogWriterFactory Current
        {
            get
            {
                lock (_locker)
                {
                    return _logWriterFactory ?? CreateLogWriterFactory();
                }
            }
        }

        public static HostLoggerConfigurator CurrentHostLoggerConfigurator
        {
            get { return _configurator ?? (_configurator = new TraceHostLoggerConfigurator()); }
        }

        static LogWriterFactory CreateLogWriterFactory()
        {
            _logWriterFactory = CurrentHostLoggerConfigurator.CreateLogWriterFactory();

            return _logWriterFactory;
        }

        public static LogWriter Get<T>()
            where T : class
        {
            return Get(typeof(T).GetTypeName());
        }

        public static LogWriter Get(Type type)
        {
            return Get(type.GetTypeName());
        }

        public static LogWriter Get(string name)
        {
            return Current.Get(name);
        }

        public static void UseLogger(HostLoggerConfigurator configurator)
        {
            lock (_locker)
            {
                _configurator = configurator;

                LogWriterFactory logger = _configurator.CreateLogWriterFactory();

                if (_logWriterFactory != null)
                    _logWriterFactory.Shutdown();
                _logWriterFactory = null;

                _logWriterFactory = logger;
            }
        }

        public static void Shutdown()
        {
            lock (_locker)
            {
                if (_logWriterFactory != null)
                {
                    _logWriterFactory.Shutdown();
                    _logWriterFactory = null;
                }
            }
        }
    }
}