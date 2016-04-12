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
    class TokenElement :
        ITokenElement
    {
        public TokenElement(string token)
        {
            Token = token;
        }

        public string Token { get; }

        public override string ToString()
        {
            return "TOKEN: " + Token;
        }

        public bool Equals(TokenElement other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Token, Token);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(TokenElement))
                return false;
            return Equals((TokenElement)obj);
        }

        public override int GetHashCode()
        {
            return Token?.GetHashCode() ?? 0;
        }

        public static ICommandLineElement New(string token)
        {
            return new TokenElement(token);
        }
    }
}