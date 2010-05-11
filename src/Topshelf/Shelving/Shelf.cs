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
namespace Topshelf.Shelving
{
    using System;
    using System.IO;
    using System.Linq;

    public class Shelf
    {
        public Shelf()
        {
            Initialize();
        }

        public void Initialize()
        {
            //how to bootstap? do I assume an interface?
            File.WriteAllText(".\\bob.txt", WellknownAddresses.CurrentShelfAddress.ToString());
        }

        static Type FindBootstrapperImplementation()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsInterface == false)
                .Where(x => typeof (Bootstrapper).IsAssignableFrom(x))
                .FirstOrDefault();

            if (type == null)
                throw new InvalidOperationException("The bootstrapper was not found.");
            return type;
        }
    }
}