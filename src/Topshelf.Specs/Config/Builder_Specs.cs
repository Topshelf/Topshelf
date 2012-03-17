// Copyright 2007-2012 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// his file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Specs.Config
{
    using Builders;
    using HostConfigurators;
    using NUnit.Framework;


    [TestFixture]
    public class Builder_Specs
    {
        [Test]
        public void matches_should_work()
        {
            var hit = false;
            ServiceDescription description = new WindowsServiceDescription("x","a");
            var x = new HelpBuilder(description);
            x.Match<HelpBuilder>(t=>
                {
                    hit = true;
                });
            hit.ShouldBeTrue();
            
        }

        [Test]
        public void should_not_hit_on_mismatch()
        {
            var hit = false;
            ServiceDescription description = new WindowsServiceDescription("x", "a");
            var x = new HelpBuilder(description);
            x.Match<InstallBuilder>(t =>
            {
                hit = true;
            });
            hit.ShouldBeFalse();

        }
    }
}