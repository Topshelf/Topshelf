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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class CommandLineElementParser<TResult> :
        AbstractParser<IEnumerable<ICommandLineElement>>,
        ICommandLineElementParser<TResult>
    {
        readonly IList<Parser<IEnumerable<ICommandLineElement>, TResult>> _parsers;

        public CommandLineElementParser()
        {
            _parsers = new List<Parser<IEnumerable<ICommandLineElement>, TResult>>();

            All = from element in _parsers.FirstMatch() select element;
        }

        public Parser<IEnumerable<ICommandLineElement>, ICommandLineElement> AnyElement
        {
            get
            {
                return input => input.Any()
                                    ? new Result<IEnumerable<ICommandLineElement>, ICommandLineElement>(input.First(),
                                          input.Skip(1))
                                    : null;
            }
        }

        public Parser<IEnumerable<ICommandLineElement>, TResult> All { get; set; }

        public void Add(Parser<IEnumerable<ICommandLineElement>, TResult> parser)
        {
            _parsers.Add(parser);
        }

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(DefinitionElement)
                   select (IDefinitionElement)c;
        }

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition(string key)
        {
            return from def in Definition()
                   where def.Key == key
                   select def;
        }

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definitions(params string[] keys)
        {
            return from def in Definition()
                   where keys.Contains(def.Key)
                   select def;
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(SwitchElement)
                   select (ISwitchElement)c;
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch(string key)
        {
            return from sw in Switch()
                   where string.Equals(sw.Key, key, StringComparison.OrdinalIgnoreCase)
                   select sw;
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switches(params string[] keys)
        {
            return from sw in Switch()
                   where keys.Contains(sw.Key)
                   select sw;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(ArgumentElement)
                   select (IArgumentElement)c;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(string value)
        {
            return from arg in Argument()
                   where string.Equals(arg.Id, value, StringComparison.OrdinalIgnoreCase)
                   select arg;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(Predicate<IArgumentElement> pred)
        {
            return from arg in Argument()
                   where pred(arg)
                   select arg;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> ValidPath()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(ArgumentElement)
                   where IsValidPath(((ArgumentElement)c).Id)
                   select (IArgumentElement)c;
        }

        public IEnumerable<TResult> Parse(IEnumerable<ICommandLineElement> elements)
        {
            Result<IEnumerable<ICommandLineElement>, TResult> result = All(elements);
            while (result != null)
            {
                yield return result.Value;

                result = All(result.Rest);
            }
        }

        static bool IsValidPath(string path)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Directory.GetCurrentDirectory(), path);

            string directoryName = Path.GetDirectoryName(path) ?? path;
            if (!Directory.Exists(directoryName))
                return false;

            return true;
        }
    }
}