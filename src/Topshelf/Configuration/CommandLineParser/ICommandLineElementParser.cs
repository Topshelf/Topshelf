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

    /// <summary>
    ///   Used to configure the command line element parser
    /// </summary>
    /// <typeparam name="TResult"> The type of object returned as a result of the parse </typeparam>
    interface ICommandLineElementParser<TResult>
    {
        /// <summary>
        ///   Adds a new pattern to the parser
        /// </summary>
        /// <param name="parser"> The pattern to match and return the resulting object </param>
        void Add(Parser<IEnumerable<ICommandLineElement>, TResult> parser);
        
        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument();
        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(string value);
        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(Predicate<IArgumentElement> pred);

        Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition();
        Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition(string key);
        Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definitions(params string[] keys);

        Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch();
        Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch(string key);
        Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switches(params string[] keys);

        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> ValidPath();
    }
}