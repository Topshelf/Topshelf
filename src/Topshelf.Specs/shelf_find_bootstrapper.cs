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

namespace Topshelf.Specs
{
    using System;
    using Magnum.TestFramework;
    using Model;
    using NUnit.Framework;


    [Scenario]
    public class shelf_find_bootstrapper
    {
        Type _foundType;

        [When]
        public void a_null_is_passed_into_find()
        {
            _foundType = Shelf.FindBootstrapperImplementationType(null);
        }

        [Test]
        public void should_find_from_appconfig()
        {
            _foundType.ShouldEqual(typeof(TestAppDomainBootsrapper));
        }
    }
}