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
namespace Topshelf.CommandLineParser
{
    class SwitchElement :
        ISwitchElement
    {
        public SwitchElement(char key)
            : this(key.ToString())
        {
        }

        public SwitchElement(string key)
            : this(key, true)
        {
        }

        public SwitchElement(string key, bool value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public bool Value { get; }

        public override string ToString()
        {
            return "SWITCH: " + Key + " (" + Value + ")";
        }

        public bool Equals(SwitchElement other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Key, Key) && other.Value.Equals(Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(SwitchElement))
                return false;
            return Equals((SwitchElement)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key?.GetHashCode() ?? 0)*397) ^ Value.GetHashCode();
            }
        }

        public static ICommandLineElement New(char key)
        {
            return new SwitchElement(key);
        }

        public static ICommandLineElement New(string key)
        {
            return new SwitchElement(key);
        }

        public static ICommandLineElement New(char key, bool value)
        {
            return new SwitchElement(key.ToString(), value);
        }
    }
}