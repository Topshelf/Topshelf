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
    using System.Linq;

    public abstract class TextCommandLineParser<TInput> :
        AbstractCharacterParser<TInput>
    {
        protected TextCommandLineParser()
        {
            Whitespace = Rep(Char(' ').Or(Char('\t').Or(Char('\n')).Or(Char('\r'))));
            NewLine = Rep(Char('\r').Or(Char('\n')));

            EscChar = (from bs in Char('\\')
                       from ch in Char('\\').Or(Char('\"')).Or(Char('-')).Or(Char('/')).Or(Char('\''))
                       select ch)
                .Or(from ch in Char(x => x != '"') select ch);

            Id = from w in Whitespace
                 from c in Char(char.IsLetter)
                 from cs in Rep(Char(char.IsLetterOrDigit))
                 select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

            Key = from w in Whitespace
                  from c in Char(char.IsLetter)
                  from cs in Rep(Char(char.IsLetterOrDigit).Or(Char('.')))
                  select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

            Value = (from symbol in Rep(Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation)))
                     select symbol.Aggregate("", (s, ch) => s + ch));

            Definition = (from w in Whitespace
                          from c in Char('-').Or(Char('/'))
                          from key in Id
                          from eq in Char(':').Or(Char('='))
                          from value in Value
                          select DefinitionElement.New(key, value))
                .Or(from w in Whitespace
                    from c in Char('-').Or(Char('/'))
                    from key in Id
                    from ws in Whitespace
                    from oq in Char('"')
                    from value in Rep(EscChar)
                    from cq in Char('"')
                    select DefinitionElement.New(key, value.Aggregate("", (s, ch) => s + ch)));

            EmptyDefinition = (from w in Whitespace
                               from c in Char('-').Or(Char('/'))
                               from key in Id
                               from ws in Whitespace
                               select DefinitionElement.New(key, ""));

            Argument = from w in Whitespace
                       from c in Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation))
                       from cs in Rep(Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation)))
                       select ArgumentElement.New(cs.Aggregate(c.ToString(), (s, ch) => s + ch));

            Switch = (from w in Whitespace
                      from c in Char('-').Or(Char('/'))
                      from arg in Char(char.IsLetterOrDigit)
                      from non in Rep(Char(char.IsLetterOrDigit))
                      where non.Count() == 0
                      select SwitchElement.New(arg))
                .Or(from w in Whitespace
                    from c in Char('-').Or(Char('/'))
                    from arg in Char(char.IsLetterOrDigit)
                    from n in Char('-')
                    select SwitchElement.New(arg, false))
                .Or(from w in Whitespace
                    from c1 in Char('-')
                    from c2 in Char('-')
                    from arg in Id
                    select SwitchElement.New(arg));

            Token = from w in Whitespace
                    from o in Char('[')
                    from t in Key
                    from c in Char(']')
                    select TokenElement.New(t);

            All =
                (from element in Definition select element)
                    .Or(from element in Switch select element)
                    .Or(from element in EmptyDefinition select element)
                    .Or(from element in Token select element)
                    .Or(from element in Argument select element);
        }


        Parser<TInput, char[]> Whitespace { get; set; }
        Parser<TInput, char[]> NewLine { get; set; }

        Parser<TInput, char> EscChar { get; set; }

        Parser<TInput, string> Id { get; set; }
        Parser<TInput, string> Key { get; set; }
        Parser<TInput, string> Value { get; set; }

        Parser<TInput, ICommandLineElement> Definition { get; set; }
        Parser<TInput, ICommandLineElement> EmptyDefinition { get; set; }
        Parser<TInput, ICommandLineElement> Argument { get; set; }
        Parser<TInput, ICommandLineElement> Token { get; set; }
        Parser<TInput, ICommandLineElement> Switch { get; set; }
        public Parser<TInput, ICommandLineElement> All { get; private set; }
    }
}