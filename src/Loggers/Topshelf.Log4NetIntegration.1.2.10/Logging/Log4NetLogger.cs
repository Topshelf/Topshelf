// Copyright 2007-2011 The Apache Software Foundation.
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

namespace Topshelf.Logging
{
    using System.IO;
    using System.Reflection;
    using log4net;
    using log4net.Config;


    public class Log4NetLogger :
        ILogger
    {
        public ILog Get(string name)
        {
            return new Log4NetLog(LogManager.GetLogger(name));
        }

        public void Shutdown()
        {
            LogManager.Shutdown();
        }

        public static void Use()
        {
            Logger.UseLogger(new Log4NetLogger());
        }

        public static void Use(string configFileName)
        {
            Use();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path == null)
                return;

            string file = Path.Combine(path, configFileName);

            var configFile = new FileInfo(file);
            if (!configFile.Exists)
                return;

            XmlConfigurator.Configure(configFile);
        }
    }
}