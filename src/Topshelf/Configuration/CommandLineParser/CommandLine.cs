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
    using System.Linq;

    /// <summary>
    ///   Tools for parsing the command line
    /// </summary>
    static class CommandLine
    {
        static readonly StringCommandLineParser _parser = new StringCommandLineParser();

        /// <summary>
        ///   Gets the command line from the Environment.CommandLine, removing the application name if present
        /// </summary>
        /// <returns> The complete, unparsed command line that was specified when the program was executed </returns>
        public static string GetUnparsedCommandLine()
        {
            string line = Environment.CommandLine;

            string applicationPath = Environment.GetCommandLineArgs().First();

            if (line == applicationPath)
                return "";

            if (line.Substring(0, applicationPath.Length) == applicationPath)
                return line.Substring(applicationPath.Length);

            string quotedApplicationPath = "\"" + applicationPath + "\"";

            if (line.Substring(0, quotedApplicationPath.Length) == quotedApplicationPath)
                return line.Substring(quotedApplicationPath.Length);

            return line;
        }

        /// <summary>
        ///   Parses the command line
        /// </summary>
        /// <param name="commandLine"> The command line to parse </param>
        /// <returns> The command line elements that were found </returns>
        static IEnumerable<ICommandLineElement> Parse(string commandLine)
        {
            Result<string, ICommandLineElement> result = _parser.All(commandLine);
            while (result != null)
            {
                yield return result.Value;

                string rest = result.Rest;

                result = _parser.All(rest);
            }
        }

        public static IEnumerable<T> Parse<T>(Action<ICommandLineElementParser<T>> initializer)
        {
            return Parse(initializer, GetUnparsedCommandLine());
        }

        /// <summary>
        ///   Parses the command line and matches any specified patterns
        /// </summary>
        /// <typeparam name="T"> The output type of the parser </typeparam>
        /// <param name="commandLine"> The command line text </param>
        /// <param name="initializer"> Used by the caller to add patterns and object generators </param>
        /// <returns> The elements that were found on the command line </returns>
        public static IEnumerable<T> Parse<T>(Action<ICommandLineElementParser<T>> initializer, string commandLine)
        {
            var elementParser = new CommandLineElementParser<T>();

            initializer?.Invoke(elementParser);

            return elementParser.Parse(Parse(commandLine));
        }
    }
}