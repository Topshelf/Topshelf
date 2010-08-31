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
namespace Topshelf.Specs.Configuration
{
    using System.ServiceProcess;
    using NUnit.Framework;
    using Topshelf.Configuration;

    [TestFixture]
    public class Credential_Specs
    {
        [Test]
        public void Equality()
        {
            Credentials oneA = new Credentials("a", "a", ServiceAccount.LocalService);
            Credentials oneB = new Credentials("a", "a", ServiceAccount.LocalService);
            Credentials two = new Credentials("2", "a", ServiceAccount.LocalService);
            Credentials three = new Credentials("a", "2", ServiceAccount.LocalService);
            Credentials four = new Credentials("a", "a", ServiceAccount.User);

            oneA.Equals(oneA)
                .ShouldBeTrue("instance equality");

            oneA.Equals(oneB)
                .ShouldBeTrue("value equality");

            oneA.Equals(two)
                .ShouldBeFalse("username");

            oneA.Equals(three)
                .ShouldBeFalse("password");

            oneA.Equals(four)
                .ShouldBeFalse("service account");

            oneA.Equals(null)
                .ShouldBeFalse("null");
        }
    }
}