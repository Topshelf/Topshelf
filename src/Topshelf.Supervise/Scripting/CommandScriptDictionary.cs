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
namespace Topshelf.Supervise.Scripting
{
    using System.Collections.Generic;

    public class CommandScriptDictionary :
        Dictionary<string, object>
    {
        public T Get<T>(string key)
        {
            object value;
            if (TryGetValue(key, out value))
            {
                return (T)value;
            }

            throw new KeyNotFoundException("The result key was not found: " + key);
        }

        public bool TryGetValue<T>(out T value)
        {
            string key = typeof(T).FullName ?? typeof(T).Name;

            object val;
            if (base.TryGetValue(key, out val))
            {
                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            object val;
            if (base.TryGetValue(key, out val))
            {
                value = (T)val;
                return true;
            }

            value = default(T);
            return false;
        }

        public T Get<T>()
        {
            string key = typeof(T).FullName ?? typeof(T).Name;

            return Get<T>(key);
        }

        public void Add<T>(T value)
        {
            string key = typeof(T).FullName ?? typeof(T).Name;

            Add(key, value);
        }

        public void Set<T>(T value)
        {
            string key = typeof(T).FullName ?? typeof(T).Name;

            Remove(key);

            Add(key, value);
        }
    }
}