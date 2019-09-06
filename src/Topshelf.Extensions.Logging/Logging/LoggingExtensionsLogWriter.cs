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
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Internal;

    /// <summary>
    /// Implements a Topshelf <see cref="LogWriter"/> for Microsoft extensions for logging.
    /// </summary>
    /// <seealso cref="Topshelf.Logging.LogWriter" />
    public class LoggingExtensionsLogWriter : LogWriter
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExtensionsLogWriter"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public LoggingExtensionsLogWriter(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is debug enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is debug enabled; otherwise, <see langword="false" />.</value>
        public bool IsDebugEnabled => this.logger.IsEnabled(LogLevel.Debug);

        /// <summary>
        /// Gets a value indicating whether this instance is information enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is information enabled; otherwise, <see langword="false" />.</value>
        public bool IsInfoEnabled => this.logger.IsEnabled(LogLevel.Information);

        /// <summary>
        /// Gets a value indicating whether this instance is warn enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is warn enabled; otherwise, <see langword="false" />.</value>
        public bool IsWarnEnabled => this.logger.IsEnabled(LogLevel.Warning);

        /// <summary>
        /// Gets a value indicating whether this instance is error enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is error enabled; otherwise, <see langword="false" />.</value>
        public bool IsErrorEnabled => this.logger.IsEnabled(LogLevel.Error);

        /// <summary>
        /// Gets a value indicating whether this instance is fatal enabled.
        /// </summary>
        /// <value><see langword="true" /> if this instance is fatal enabled; otherwise, <see langword="false" />.</value>
        public bool IsFatalEnabled => this.logger.IsEnabled(LogLevel.Critical);

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        public void Log(LoggingLevel level, object obj) => this.logger.Log(ToLogLevel(level), 0, obj, null, (s, e) => s.ToString());

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Log(LoggingLevel level, object obj, Exception exception) => this.logger.Log(ToLogLevel(level), 0, obj, exception, (s, e) => s.ToString());

        /// <summary>
        /// Logs the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="messageProvider">The message provider.</param>
        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (this.logger.IsEnabled(ToLogLevel(level)))
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate

                this.Log(level, @object);
            }
        }

        /// <summary>
        /// Logs the format.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args) => this.LogFormat(level, format, args);

        /// <summary>
        /// Logs the format.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void LogFormat(LoggingLevel level, string format, params object[] args) => this.Log(level, new FormattedLogValues(format, args));

        /// <summary>
        /// Debugs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Debug(object obj) => this.Debug(obj, null);

        /// <summary>
        /// Debugs the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Debug(object obj, Exception exception)
        {
            if (obj is string text)
            {
                this.logger.LogDebug(0, exception, text);
            }
            else
            {
                this.logger.LogDebug(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Debugs the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (this.IsDebugEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                this.Debug(@object);
            }
        }

        /// <summary>
        /// Debugs the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args) => this.DebugFormat(format, args);

        /// <summary>
        /// Debugs the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void DebugFormat(string format, params object[] args) => this.logger.LogDebug(format, args);

        /// <summary>
        /// Informations the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Info(object obj) => this.Info(obj, null);

        /// <summary>
        /// Informations the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Info(object obj, Exception exception)
        {
            if (obj is string text)
            {
                this.logger.LogInformation(0, exception, text);
            }
            else
            {
                this.logger.LogInformation(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Informations the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (this.IsInfoEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                this.Info(@object);
            }
        }

        /// <summary>
        /// Informations the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args) => this.InfoFormat(format, args);

        /// <summary>
        /// Informations the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void InfoFormat(string format, params object[] args) => this.logger.LogInformation(format, args);

        /// <summary>
        /// Warns the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Warn(object obj)
        {
            if (obj is string text)
            {
                this.logger.LogWarning(text);
            }
            else
            {
                this.logger.LogWarning("{obj}", obj);
            }
        }

        /// <summary>
        /// Warns the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Warn(object obj, Exception exception)
        {
            if (obj is string text)
            {
                this.logger.LogWarning(0, exception, text);
            }
            else
            {
                this.logger.LogWarning(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Warns the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (this.IsWarnEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                this.Warn(@object);
            }
        }

        /// <summary>
        /// Warns the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args) => this.logger.LogWarning(format, args);

        /// <summary>
        /// Warns the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WarnFormat(string format, params object[] args) => this.logger.LogWarning(format, args);

        /// <summary>
        /// Errors the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Error(object obj)
        {
            if (obj is string text)
            {
                this.logger.LogError(text);
            }
            else
            {
                this.logger.LogError("{obj}", obj);
            }
        }

        /// <summary>
        /// Errors the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Error(object obj, Exception exception)
        {
            if (obj is string text)
            {
                this.logger.LogError(0, exception, text);
            }
            else
            {
                this.logger.LogError(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Errors the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (this.IsErrorEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                this.Error(@object);
            }
        }

        /// <summary>
        /// Errors the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args) => this.ErrorFormat(format, args);

        /// <summary>
        /// Errors the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void ErrorFormat(string format, params object[] args) => this.logger.LogError(format, args);

        /// <summary>
        /// Fatals the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Fatal(object obj)
        {
            if (obj is string text)
            {
                this.logger.LogCritical(text);
            }
            else
            {
                this.logger.LogCritical("{obj}", obj);
            }
        }

        /// <summary>
        /// Fatals the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="exception">The exception.</param>
        public void Fatal(object obj, Exception exception)
        {
            if (obj is string text)
            {
                this.logger.LogCritical(0, exception, text);
            }
            else
            {
                this.logger.LogCritical(0, exception, "{obj}", obj);
            }
        }

        /// <summary>
        /// Fatals the specified message provider.
        /// </summary>
        /// <param name="messageProvider">The message provider.</param>
        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (this.IsFatalEnabled)
            {
                System.Diagnostics.Debug.Assert(messageProvider != null, nameof(messageProvider) + " is null.");
#pragma warning disable CC0031 // Check for null before calling a delegate
                var @object = messageProvider();
#pragma warning restore CC0031 // Check for null before calling a delegate
                this.Fatal(@object);
            }
        }

        /// <summary>
        /// Fatals the format.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args) => this.FatalFormat(format, args);

        /// <summary>
        /// Fatals the format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void FatalFormat(string format, params object[] args) => this.logger.LogCritical(format, args);

        /// <summary>
        /// To the log level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>LogLevel.</returns>
        private static LogLevel ToLogLevel(LoggingLevel level)
        {
            if (level == LoggingLevel.Fatal)
            {
                return LogLevel.Critical;
            }
            else if (level == LoggingLevel.Error)
            {
                return LogLevel.Error;
            }
            else if (level == LoggingLevel.Warn)
            {
                return LogLevel.Warning;
            }
            else if (level == LoggingLevel.Info)
            {
                return LogLevel.Information;
            }
            else if (level >= LoggingLevel.Debug)
            {
                return LogLevel.Debug;
            }
            else
            {
                return LogLevel.None;
            }
        }
    }
}
