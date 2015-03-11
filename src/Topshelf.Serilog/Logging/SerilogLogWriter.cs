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
    using System.Globalization;
    using Serilog;
    using Serilog.Events;

    public class SerilogLogWriter : LogWriter
    {
        readonly ILogger _logger;

        public SerilogLogWriter(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsDebugEnabled
        {
            get { return _logger.IsEnabled(LogEventLevel.Debug); }
        }

        public bool IsInfoEnabled
        {
            get { return _logger.IsEnabled(LogEventLevel.Information); }
        }

        public bool IsWarnEnabled
        {
            get { return _logger.IsEnabled(LogEventLevel.Warning); }
        }

        public bool IsErrorEnabled
        {
            get { return _logger.IsEnabled(LogEventLevel.Error); }
        }

        public bool IsFatalEnabled
        {
            get { return _logger.IsEnabled(LogEventLevel.Fatal); }
        }

        public void Log(LoggingLevel level, object obj)
        {
            if (level == LoggingLevel.Fatal)
                Fatal(obj);
            else if (level == LoggingLevel.Error)
                Error(obj);
            else if (level == LoggingLevel.Warn)
                Warn(obj);
            else if (level == LoggingLevel.Info)
                Info(obj);
            else if (level >= LoggingLevel.Debug)
                Debug(obj);
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            if (level == LoggingLevel.Fatal)
                Fatal(obj, exception);
            else if (level == LoggingLevel.Error)
                Error(obj, exception);
            else if (level == LoggingLevel.Warn)
                Warn(obj, exception);
            else if (level == LoggingLevel.Info)
                Info(obj, exception);
            else if (level >= LoggingLevel.Debug)
                Debug(obj, exception);
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (level == LoggingLevel.Fatal)
                Fatal(messageProvider);
            else if (level == LoggingLevel.Error)
                Error(messageProvider);
            else if (level == LoggingLevel.Warn)
                Warn(messageProvider);
            else if (level == LoggingLevel.Info)
                Info(messageProvider);
            else if (level >= LoggingLevel.Debug)
                Debug(messageProvider);
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (level == LoggingLevel.Fatal)
                FatalFormat(formatProvider, format, args);
            else if (level == LoggingLevel.Error)
                ErrorFormat(formatProvider, format, args);
            else if (level == LoggingLevel.Warn)
                WarnFormat(formatProvider, format, args);
            else if (level == LoggingLevel.Info)
                InfoFormat(formatProvider, format, args);
            else if (level >= LoggingLevel.Debug)
                DebugFormat(formatProvider, format, args);
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            if (level == LoggingLevel.Fatal)
                FatalFormat(CultureInfo.InvariantCulture, format, args);
            else if (level == LoggingLevel.Error)
                ErrorFormat(CultureInfo.InvariantCulture, format, args);
            else if (level == LoggingLevel.Warn)
                WarnFormat(CultureInfo.InvariantCulture, format, args);
            else if (level == LoggingLevel.Info)
                InfoFormat(CultureInfo.InvariantCulture, format, args);
            else if (level >= LoggingLevel.Debug)
                DebugFormat(CultureInfo.InvariantCulture, format, args);
        }

        public void Debug(object obj)
        {
            if (obj is string)
            {
                _logger.Debug((string)obj);
                return;
            }
            _logger.Debug("{@obj}", obj);
        }

        public void Debug(object obj, Exception exception)
        {
            if (obj is string)
            {
                _logger.Debug(exception, (string)obj);
                return;
            }
            _logger.Debug(exception, "{@obj}", obj);
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled) return;

            Debug(messageProvider());
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _logger.Debug(format, args);
        }

        public void Info(object obj)
        {
            if (obj is string)
            {
                _logger.Information((string)obj);
                return;
            }
            _logger.Information("{@obj}", obj);
        }

        public void Info(object obj, Exception exception)
        {
            if (obj is string)
            {
                _logger.Information(exception, (string)obj);
                return;
            }
            _logger.Information(exception, "{@obj}", obj);
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled) return;

            Info(messageProvider());
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.Information(format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _logger.Information(format, args);
        }

        public void Warn(object obj)
        {
            if (obj is string)
            {
                _logger.Warning((string)obj);
                return;
            }
            _logger.Warning("{@obj}", obj);
        }

        public void Warn(object obj, Exception exception)
        {
            if (obj is string)
            {
                _logger.Warning(exception, (string)obj);
                return;
            }
            _logger.Warning(exception, "{@obj}", obj);
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled) return;

            Warn(messageProvider());
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.Warning(format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _logger.Warning(format, args);
        }

        public void Error(object obj)
        {
            if (obj is string)
            {
                _logger.Error((string)obj);
                return;
            }
            _logger.Error("{@obj}", obj);
        }

        public void Error(object obj, Exception exception)
        {
            if (obj is string)
            {
                _logger.Error(exception, (string)obj);
                return;
            }
            _logger.Error(exception, "{@obj}", obj);
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled) return;

            Error(messageProvider());
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _logger.Error(format, args);
        }

        public void Fatal(object obj)
        {
            if (obj is string)
            {
                _logger.Fatal((string)obj);
                return;
            }
            _logger.Fatal("{@obj}", obj);
        }

        public void Fatal(object obj, Exception exception)
        {
            if (obj is string)
            {
                _logger.Fatal(exception, (string)obj);
                return;
            }
            _logger.Fatal(exception, "{@obj}", obj);
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled) return;

            Fatal(messageProvider());
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _logger.Fatal(format, args);
        }
    }
}