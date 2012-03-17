// Copyright 2007-2012 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// his file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using System.Diagnostics;
    using Internal;
    using Logging;


    public abstract class Builder : 
        HostBuilder
    {
        static readonly ILog _logger = Logger.Get<Builder>();
        readonly ServiceDescription _description;

        protected Builder([NotNull]ServiceDescription description)
        {
            if (description == null)
                throw new ArgumentNullException("description");

            _description = description;
        }

        public ServiceDescription Description
        {
            get { return _description; }
        }

        public abstract Host Build();

      
        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback != null)
            {
                if (typeof(T).IsAssignableFrom(GetType()))
                    callback(this as T);
            }
            else
            {
                _logger.Warn("Match{{T}} called with callback of null. If you are running the host "
                             + "in debug mode, the next log message will print a stack trace.");
#if DEBUG
                _logger.Warn(new StackTrace());
#endif
            }
        }
    }
}