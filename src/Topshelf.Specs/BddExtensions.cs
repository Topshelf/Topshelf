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
namespace Topshelf.Specs
{
    using System.Collections.Generic;
    using NUnit.Framework;

    public static class BddExtensions
    {
        public static void ShouldContain<T>(this IEnumerable<T> list, T value)
        {
                Assert.Contains(value, new List<T>(list));
        }

        public static void ShouldEqual<T>(this T actual, T expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ShouldNotBeNull<T>(this T actual)
        {
            Assert.IsNotNull(actual);
        }

        public static void ShouldBeTrue(this bool actual)
        {
            Assert.IsTrue(actual);
        }

        public static void ShouldBeTrue(this bool actual, string message)
        {
            Assert.IsTrue(actual, message);
        }

        public static void ShouldBeFalse(this bool actual)
        {
            Assert.IsFalse(actual);
        }

        public static void ShouldBeFalse(this bool actual, string message)
        {
            Assert.IsFalse(actual, message);
        }
    }
}