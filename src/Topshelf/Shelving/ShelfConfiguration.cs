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
namespace Topshelf.Shelving
{
    using System;
    using System.Configuration;
    using System.IO;

    public class ShelfConfiguration :
        ConfigurationSection
    {
        public static ShelfConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("ShelfConfiguration") as ShelfConfiguration;
        }

        public static ShelfConfiguration GetConfig(string fileName)
        {
            // TODO: Do I really have to do this? Make a file exist so the .config will really load?
            // This likely needs to be reconsidered - is there another way to load the config file for the app domain?
            // Can I pull it directly from the AppDomain, DomainManager, or something else? 
            
            // chop off the .config
            if (fileName.EndsWith(".config"))
                fileName = fileName.Substring(0, fileName.Length - ".config".Length);

            // if the filename doesn't exist, it won't load
            if (!File.Exists(fileName))
                File.WriteAllText(fileName, string.Empty);

            var exeConfig = ConfigurationManager.OpenExeConfiguration(fileName);
            var section = exeConfig.GetSection("ShelfConfiguration");
            return section as ShelfConfiguration;
        }

        [ConfigurationProperty("Bootstrapper", IsRequired = true)]
        public string Bootstrapper
        {
            get { return (string) this["Bootstrapper"]; }
        }

        public Type BootstrapperType
        {
            get
            {
                var value = Bootstrapper;
                return Type.GetType(value);
            }
        }

    }
}