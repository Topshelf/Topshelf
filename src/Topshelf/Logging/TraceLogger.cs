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
    using Internals.Caching;

    public class TraceLogger :
        ILogger
    {
        readonly Cache<string, TraceLog> _logs;
        readonly Cache<string, TraceSource> _sources;
        ConsoleTraceListener _listener;
        TraceSource _defaultSource;
        SourceSwitch _defaultSwitch;
        int _listenerIndex;

        public TraceLogger()
        {
            _logs = new ConcurrentCache<string, TraceLog>(CreateTraceLog);
            _sources = new ConcurrentCache<string, TraceSource>(CreateTraceSource);

            _defaultSwitch = new SourceSwitch("Topshelf");
            _defaultSwitch.Level = SourceLevels.All;

            _defaultSource = new TraceSource("Topshelf");
            _defaultSource.Switch = _defaultSwitch;

            _listener = AddDefaultConsoleTraceListener();

            _defaultSource.TraceEvent(TraceEventType.Information, 1, "Logging initialized");
        }

        ConsoleTraceListener AddDefaultConsoleTraceListener()
        {
            var listener = new ConsoleTraceListener();
            listener.Name = "Topshelf";

            _listenerIndex = _defaultSource.Listeners.Add(listener);

            return listener;
        }

        public Log Get(string name)
        {
            return _logs[name];
        }

        public void Shutdown()
        {
            Trace.Flush();

            if (_listener != null)
            {
                Trace.Listeners.Remove(_listener);

                _listener.Close();
                (_listener as IDisposable).Dispose();
                _listener = null;
            }
        }

        TraceLog CreateTraceLog(string name)
        {
            return new TraceLog(_sources[name]);
        }

        TraceSource CreateTraceSource(string name)
        {
            return _defaultSource;
            LogLevel logLevel = LogLevel.Info;
            SourceLevels sourceLevel = logLevel.SourceLevel;
            var source = new TraceSource(name, sourceLevel);
            if (IsSourceConfigured(source))
            {
                return source;
            }

            ConfigureTraceSource(source, name, sourceLevel);

            return source;
        }

        void ConfigureTraceSource(TraceSource source, string name, SourceLevels sourceLevel)
        {
            var defaultSource = _defaultSource;
            for (string parentName = ShortenName(name);
                 !string.IsNullOrEmpty(parentName);
                 parentName = ShortenName(parentName))
            {
                var parentSource = new TraceSource(parentName, sourceLevel);
                if (IsSourceConfigured(parentSource))
                {
                    defaultSource = parentSource;
                    break;
                }
            }

            source.Switch = defaultSource.Switch;
            source.Listeners.Clear();
            foreach (TraceListener listener in defaultSource.Listeners)
                source.Listeners.Add(listener);
        }

        static bool IsSourceConfigured(TraceSource source)
        {
            return source.Listeners.Count != 1
                   || !(source.Listeners[0] is DefaultTraceListener)
                   || source.Listeners[0].Name != "Topshelf";
        }

        static string ShortenName(string name)
        {
            int length = name.LastIndexOf('.');

            return length != -1
                       ? name.Substring(0, length)
                       : null;
        }
    }
}