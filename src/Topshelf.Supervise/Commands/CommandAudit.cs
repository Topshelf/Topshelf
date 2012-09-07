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
namespace Topshelf.Supervise.Commands
{
    using System;
    using System.Collections.Generic;

    public class CommandAudit
    {
        readonly CommandResult _result;
        readonly Type _type;

        public CommandAudit(Command command, CommandResult result)
        {
            _result = result;
            _type = command.GetType();
        }

        public CommandResult Result
        {
            get { return _result; }
        }

        public Type CommandType
        {
            get { return _type; }
        }

        public T Get<T>(string key)
        {
            object value;
            if (_result.TryGetValue(key, out value))
            {
                return (T)value;
            }

            throw new KeyNotFoundException("The result key was not found: " + key);
        }

        public T Get<T>()
        {
            string key = typeof(T).FullName ?? typeof(T).Name;

            return Get<T>(key);
        }
    }
}