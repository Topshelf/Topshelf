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

namespace Topshelf.Shelving
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;


    public class ShelfConfiguration :
        ConfigurationSection
    {
        [ConfigurationProperty("Bootstrapper", IsRequired = true)]
        public string Bootstrapper
        {
            get { return (string)this["Bootstrapper"]; }
        }

        public Type BootstrapperType
        {
            get
            {
                string value = Bootstrapper;
                return Type.GetType(value);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ShelfConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("ShelfConfiguration") as ShelfConfiguration;
        }
    }
}