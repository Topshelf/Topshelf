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
    using NLog;

    /// <summary>
    ///   A logger that wraps to NLog. See http://stackoverflow.com/questions/7412156/how-to-retain-callsite-information-when-wrapping-nlog
    /// </summary>
    public class NLogLogWriter :
        LogWriter
    {
        readonly Logger _log;

        /// <summary>
        ///   Create a new NLog logger instance.
        /// </summary>
        /// <param name="log"> </param>
        /// <param name="name"> Name of type to log as. </param>
        public NLogLogWriter(Logger log, string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            _log = log;
        }

        public bool IsDebugEnabled
        {
            get { return _log.IsDebugEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _log.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _log.IsFatalEnabled; }
        }

        public void Log(LoggingLevel level, object obj)
        {
            _log.Log(GetNLogLevel(level), obj);
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            _log.Log(GetNLogLevel(level), obj == null
                                                       ? ""
                                                       : obj.ToString(), exception);
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            _log.Log(GetNLogLevel(level), ToGenerator(messageProvider));
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format,
            params object[] args)
        {
            _log.Log(GetNLogLevel(level), formatProvider, format, args);
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            _log.Log(GetNLogLevel(level), format, args);
        }

        public void Debug(object obj)
        {
            _log.Log(LogLevel.Debug, obj);
        }

        public void Debug(object obj, Exception exception)
        {
            _log.Log(LogLevel.Debug, obj == null
                                                       ? ""
                                                       : obj.ToString(), exception);
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            _log.Debug(ToGenerator(messageProvider));
        }

        public void Info(object obj)
        {
            _log.Log(LogLevel.Info, obj);
        }

        public void Info(object obj, Exception exception)
        {
            _log.Log(LogLevel.Info, obj == null
                                                      ? ""
                                                      : obj.ToString(), exception);
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            _log.Info(ToGenerator(messageProvider));
        }

        public void Warn(object obj)
        {
            _log.Log(LogLevel.Warn, obj);
        }

        public void Warn(object obj, Exception exception)
        {
            _log.Log(LogLevel.Warn, obj == null
                                                      ? ""
                                                      : obj.ToString(), exception);
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            _log.Warn(ToGenerator(messageProvider));
        }

        public void Error(object obj)
        {
            _log.Log(LogLevel.Error, obj);
        }

        public void Error(object obj, Exception exception)
        {
            _log.Log(LogLevel.Error, obj == null
                                                       ? ""
                                                       : obj.ToString(), exception);
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            _log.Error(ToGenerator(messageProvider));
        }

        public void Fatal(object obj)
        {
            _log.Log(LogLevel.Fatal, obj);
        }

        public void Fatal(object obj, Exception exception)
        {
            _log.Log(LogLevel.Fatal, obj == null
                                                       ? ""
                                                       : obj.ToString(), exception);
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            _log.Fatal(ToGenerator(messageProvider));
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Debug, formatProvider, format, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Debug, format, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Info, formatProvider, format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Info, format, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Warn, formatProvider, format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Warn, format, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Error, formatProvider, format, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Error, format, args);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _log.Log(LogLevel.Fatal, formatProvider, format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _log.Log(LogLevel.Fatal, format, args);
        }

        LogLevel GetNLogLevel(LoggingLevel level)
        {
            if (level == LoggingLevel.Fatal)
                return LogLevel.Fatal;
            if (level == LoggingLevel.Error)
                return LogLevel.Error;
            if (level == LoggingLevel.Warn)
                return LogLevel.Warn;
            if (level == LoggingLevel.Info)
                return LogLevel.Info;
            if (level == LoggingLevel.Debug)
                return LogLevel.Debug;
            if (level == LoggingLevel.All)
                return LogLevel.Trace;

            return LogLevel.Off;
        }

        LogMessageGenerator ToGenerator(LogWriterOutputProvider provider)
        {
            return () =>
                {
                    object obj = provider();
                    return obj == null
                               ? ""
                               : obj.ToString();
                };
        }
    }
}