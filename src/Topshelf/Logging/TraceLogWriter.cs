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
    using System.Diagnostics;
    using System.Globalization;

    public class TraceLogWriter :
        LogWriter
    {
        readonly LoggingLevel _level;
        readonly TraceSource _source;

        public TraceLogWriter(TraceSource source)
        {
            _source = source;
            _level = LoggingLevel.FromSourceLevels(source.Switch.Level);
        }

        public bool IsDebugEnabled
        {
            get { return _level >= LoggingLevel.Debug; }
        }

        public bool IsInfoEnabled
        {
            get { return _level >= LoggingLevel.Info; }
        }

        public bool IsWarnEnabled
        {
            get { return _level >= LoggingLevel.Warn; }
        }

        public bool IsErrorEnabled
        {
            get { return _level >= LoggingLevel.Error; }
        }

        public bool IsFatalEnabled
        {
            get { return _level >= LoggingLevel.Fatal; }
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            if (_level < level)
                return;

            LogInternal(level, string.Format(format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Debug(object message)
        {
            if (!IsDebugEnabled)
                return;
            Log(LoggingLevel.Debug, message, null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Debug(object message, Exception exception)
        {
            if (!IsDebugEnabled)
                return;
            Log(LoggingLevel.Debug, message, exception);
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Debug, obj, null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Info(object message)
        {
            if (!IsInfoEnabled)
                return;
            Log(LoggingLevel.Info, message, null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Info(object message, Exception exception)
        {
            if (!IsInfoEnabled)
                return;
            Log(LoggingLevel.Info, message, exception);
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Info, obj, null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Warn(object message)
        {
            if (!IsWarnEnabled)
                return;
            Log(LoggingLevel.Warn, message, null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Warn(object message, Exception exception)
        {
            if (!IsWarnEnabled)
                return;
            Log(LoggingLevel.Warn, message, exception);
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Warn, obj, null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Error(object message)
        {
            if (!IsErrorEnabled)
                return;
            Log(LoggingLevel.Error, message, null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Error(object message, Exception exception)
        {
            if (!IsErrorEnabled)
                return;
            Log(LoggingLevel.Error, message, exception);
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Error, obj, null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Fatal(object message)
        {
            if (!IsFatalEnabled)
                return;
            Log(LoggingLevel.Fatal, message, null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Fatal(object message, Exception exception)
        {
            if (!IsFatalEnabled)
                return;
            Log(LoggingLevel.Fatal, message, exception);
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Fatal, obj, null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(formatProvider, format, args), null);
        }

        public void Log(LoggingLevel level, object obj)
        {
            if (_level < level)
                return;

            LogInternal(level, obj, null);
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            if (_level < level)
                return;

            LogInternal(level, obj, exception);
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (_level < level)
                return;

            object obj = messageProvider();

            LogInternal(level, obj, null);
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (_level < level)
                return;

            LogInternal(level, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(formatProvider, format, args), exception);
        }

        void LogInternal(LoggingLevel level, object obj, Exception exception)
        {
            string message = obj == null
                                 ? ""
                                 : obj.ToString();

            if (exception == null)
                _source.TraceEvent(level.TraceEventType, 0, message);
            else
                _source.TraceData(level.TraceEventType, 0, (object)message, (object)exception);
        }
    }
}