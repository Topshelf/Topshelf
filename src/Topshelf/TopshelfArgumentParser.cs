// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf
{
    using System.Collections.Generic;
    using System.Linq;
    using Magnum.CommandLineParser;

    public static class TopshelfArgumentParser
    {
        public static TopshelfArguments Parse(string[] args)
        {
            string argv = args.Aggregate("", (l, r) => "{0} {1}".FormatWith(l, r));
            return Parse(argv);
        }
        public static TopshelfArguments Parse(string commandLine)
        {
            var result = new TopshelfArguments();

            Set(result, P(commandLine));

            return result;
        }

        static void Set(TopshelfArguments args, IEnumerable<ICommandLineElement> commandLineElements)
        {
            var command = commandLineElements
                .Take(1)
                .Select(x => (IArgumentElement) x)
                .Select(x => x.Id)
                .DefaultIfEmpty("commandline")
                .SingleOrDefault();


            args.Command = command;
            //leftovers
            args.CommandArgs = commandLineElements.Skip(1).ToList();
        }

        static IEnumerable<ICommandLineElement> P(string commandLine)
        {
            var parser = new MonadicCommandLineParser();

            return parser.Parse(commandLine);
        }
    }
}