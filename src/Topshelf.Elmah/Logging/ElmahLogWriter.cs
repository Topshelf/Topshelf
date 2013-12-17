// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using Elmah;
    using Topshelf.Logging;

    public class ElmahLogLevels
    {
        public bool IsDebugEnabled { get; set; }
        public bool IsInfoEnabled { get; set; }
        public bool IsWarnEnabled { get; set; }
        public bool IsErrorEnabled { get; set; }
        public bool IsFatalEnabled { get; set; }
    }

    public enum ElmahLogLevelsEnum
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class ElmahLogWriter :
        LogWriter
    {
        private readonly ErrorLog _log;
        private readonly ElmahLogLevels _logLevels;

        public ElmahLogWriter()
            : this(ErrorLog.GetDefault(null), null)
        { }

        public ElmahLogWriter(ElmahLogLevels logLevels)
            : this(ErrorLog.GetDefault(null), logLevels)
        { }

        public ElmahLogWriter(ErrorLog log, ElmahLogLevels logLevels)
        {
            _log = log;
            _logLevels = logLevels ?? new ElmahLogLevels()
            {
                IsDebugEnabled = true,
                IsErrorEnabled = true,
                IsFatalEnabled = true,
                IsInfoEnabled = true,
                IsWarnEnabled = true
            };
        }

        private void WriteToElmah(ElmahLogLevelsEnum logLevel, LogWriterOutputProvider messageProvider)
        {
            WriteToElmah(logLevel, messageProvider().ToString());
        }

        private void WriteToElmah(ElmahLogLevelsEnum logLevel, string format, params object[] args)
        {
            WriteToElmah(logLevel, String.Format(format, args));
        }

        private void WriteToElmah(ElmahLogLevelsEnum logLevel, string message, Exception exception = null)
        {
            var error = exception == null ? new Error() : new Error(exception);
            var type = error.Exception != null ? error.Exception.GetType().FullName : GetLogLevel(logLevel);
            error.Type = type;
            error.Message = message;
            error.Time = DateTime.Now;
            error.HostName = Environment.MachineName;
            error.Detail = exception == null ? message : exception.StackTrace;

            _log.Log(error);
        }

        private string GetLogLevel(ElmahLogLevelsEnum logLevel)
        {
            switch (logLevel)
            {
                case ElmahLogLevelsEnum.Debug:
                    return "Debug";
                case ElmahLogLevelsEnum.Info:
                    return "Info";
                case ElmahLogLevelsEnum.Warn:
                    return "Warn";
                case ElmahLogLevelsEnum.Error:
                    return "Error";
                case ElmahLogLevelsEnum.Fatal:
                    return "Fatal";
            }

            return "unknown log level";
        }

        
        public void Debug(object message)
        {
            WriteToElmah(ElmahLogLevelsEnum.Debug, message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            WriteToElmah(ElmahLogLevelsEnum.Debug, message.ToString(), exception);
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled)
                return;

            WriteToElmah(ElmahLogLevelsEnum.Debug, messageProvider);
        }

        public void DebugFormat(string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Debug, format, args);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Debug, format, args);
        }

        public void Info(object message)
        {
            WriteToElmah(ElmahLogLevelsEnum.Info, message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            WriteToElmah(ElmahLogLevelsEnum.Info, message.ToString(), exception);
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled)
                return;

            WriteToElmah(ElmahLogLevelsEnum.Info, messageProvider);
        }

        public void InfoFormat(string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Info, format, args);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Info, format, args);
        }

        public void Warn(object message)
        {
            WriteToElmah(ElmahLogLevelsEnum.Warn, message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            WriteToElmah(ElmahLogLevelsEnum.Warn, message.ToString(), exception);
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled)
                return;

            WriteToElmah(ElmahLogLevelsEnum.Warn, messageProvider);
        }

        public void WarnFormat(string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Warn, format, args);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Warn, format, args);
        }

        public void Error(object message)
        {
            WriteToElmah(ElmahLogLevelsEnum.Error, message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            WriteToElmah(ElmahLogLevelsEnum.Error, message.ToString(), exception);
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled)
                return;

            WriteToElmah(ElmahLogLevelsEnum.Error, messageProvider);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Error, format, args);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Error, format, args);
        }

        public void Fatal(object message)
        {
            WriteToElmah(ElmahLogLevelsEnum.Fatal, message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            WriteToElmah(ElmahLogLevelsEnum.Fatal, message.ToString(), exception);
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled)
                return;
            
            WriteToElmah(ElmahLogLevelsEnum.Fatal, messageProvider);
        }

        public void FatalFormat(string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Fatal, format, args);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            WriteToElmah(ElmahLogLevelsEnum.Fatal, format, args);
        }

        public bool IsDebugEnabled
        {
            get { return _logLevels.IsDebugEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _logLevels.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _logLevels.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _logLevels.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _logLevels.IsFatalEnabled; }
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
    }
}