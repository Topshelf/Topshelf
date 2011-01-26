// Copyright 2007-2010 The Apache Software Foundation.
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
    using System;
    using Model;
    using Shelving;
    using Topshelf.Configuration.Dsl;

    public class TypeCoercionPlay
    {
        public void TestIt()
        {
            Type t = Shelf.FindBootstrapperImplementationType(null);
            Console.WriteLine(t);
        }
    }

    public class BS : Bootstrapper<string>
    {
        public void InitializeHostedService(IServiceConfigurator<string> cfg)
        {
            throw new NotImplementedException();
        }
    }
}