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

    static class MonadParserExtensions
    {
        public static Parser<TInput, TValue> Where<TInput, TValue>(this Parser<TInput, TValue> parser,
            Func<TValue, bool> pred)
        {
            return input =>
                {
                    Result<TInput, TValue> result = parser(input);
                    if (result == null || !pred(result.Value))
                        return null;

                    return result;
                };
        }

        public static Parser<TInput, TSelect> Select<TInput, TValue, TSelect>(this Parser<TInput, TValue> parser,
            Func<TValue, TSelect> selector)
        {
            return input =>
                {
                    Result<TInput, TValue> result = parser(input);
                    if (result == null)
                        return null;

                    return new Result<TInput, TSelect>(selector(result.Value), result.Rest);
                };
        }

        public static Parser<TInput, TSelect> SelectMany<TInput, TValue, TIntermediate, TSelect>(
            this Parser<TInput, TValue> parser, Func<TValue, Parser<TInput, TIntermediate>> selector,
            Func<TValue, TIntermediate, TSelect> projector)
        {
            return input =>
                {
                    Result<TInput, TValue> result = parser(input);
                    if (result == null)
                        return null;

                    TValue val = result.Value;
                    Result<TInput, TIntermediate> nextResult = selector(val)(result.Rest);
                    if (nextResult == null)
                        return null;

                    return new Result<TInput, TSelect>(projector(val, nextResult.Value), nextResult.Rest);
                };
        }

        public static Parser<TInput, TValue> Or<TInput, TValue>(this Parser<TInput, TValue> first,
            Parser<TInput, TValue> second)
        {
            return input => first(input) ?? second(input);
        }

        public static Parser<TInput, TValue> FirstMatch<TInput, TValue>(this IEnumerable<Parser<TInput, TValue>> options)
        {
            return input =>
                {
                    return options
                        .Select(option => option(input))
                        .Where(result => result != null)
                        .FirstOrDefault();
                };
        }

        public static Parser<TInput, TSecondValue> And<TInput, TFirstValue, TSecondValue>(
            this Parser<TInput, TFirstValue> first,
            Parser<TInput, TSecondValue> second)
        {
            return input => second(first(input).Rest);
        }
    }
}