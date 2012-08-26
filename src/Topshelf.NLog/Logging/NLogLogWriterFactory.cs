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
    using NLog;

    public class NLogLogWriterFactory :
        LogWriterFactory
    {
        readonly LogFactory _logFactory;

        public NLogLogWriterFactory(LogFactory logFactory)
        {
            _logFactory = logFactory;
        }

        public NLogLogWriterFactory()
            : this(new LogFactory())
        {
        }

        public LogWriter Get(string name)
        {
            return new NLogLogWriter(_logFactory.GetLogger(name), name);
        }

        public void Shutdown()
        {
            _logFactory.Dispose();
        }

        public static void Use()
        {
            HostLogger.UseLogger(new NLogLogWriterFactory());
        }
    }
}