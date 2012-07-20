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
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionForCommandLineElementParsers
    {
        public static Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Optional(
            this Parser<IEnumerable<ICommandLineElement>, ISwitchElement> source, string key, bool defaultValue)
        {
            return input =>
                {
                    IEnumerable<ICommandLineElement> query = input
                        .Where(x => x.GetType() == typeof(SwitchElement))
                        .Where(x => ((SwitchElement)x).Key == key);

                    if (query.Any())
                        return
                            new Result<IEnumerable<ICommandLineElement>, ISwitchElement>(
                                query.First() as ISwitchElement, input.Except(query));

                    return
                        new Result<IEnumerable<ICommandLineElement>, ISwitchElement>(
                            new SwitchElement(key, defaultValue), input);
                };
        }

        public static Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Optional(
            this Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> source, string key, string defaultValue)
        {
            return input =>
                {
                    IEnumerable<ICommandLineElement> query = input
                        .Where(x => x.GetType() == typeof(DefinitionElement))
                        .Where(x => ((DefinitionElement)x).Key == key);

                    if (query.Any())
                        return
                            new Result<IEnumerable<ICommandLineElement>, IDefinitionElement>(
                                query.First() as IDefinitionElement, input.Except(query));

                    return
                        new Result<IEnumerable<ICommandLineElement>, IDefinitionElement>(
                            new DefinitionElement(key, defaultValue), input);
                };
        }
    }
}