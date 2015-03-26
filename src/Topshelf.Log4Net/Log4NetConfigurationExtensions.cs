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
namespace Topshelf
{
    using HostConfigurators;
    using Logging;

    /// <summary>
    ///   Extensions for configuring Logging for log4net
    /// </summary>
    public static class Log4NetConfigurationExtensions
    {
        /// <summary>
        ///   Specify that you want to use the Log4net logging engine.
        /// </summary>
        /// <param name="configurator"> </param>
        public static void UseLog4Net(this HostConfigurator configurator)
        {
            Log4NetLogWriterFactory.Use();
        }

        /// <summary>
        ///   Specify that you want to use the Log4net logging engine.
        /// </summary>
        /// <param name="configurator"> </param>
        /// <param name="configFileName"> The name of the log4net xml configuration file </param>
        public static void UseLog4Net(this HostConfigurator configurator, string configFileName)
        {
            Log4NetLogWriterFactory.Use(configFileName);
        }

        /// <summary>
        ///   Specify that you want to use the Log4net logging engine.
        /// </summary>
        /// <param name="configurator"> </param>
        /// <param name="configFileName"> The name of the log4net xml configuration file </param>
        /// <param name="watchFile"> Should log4net watch the config file? </param>
        public static void UseLog4Net(this HostConfigurator configurator, string configFileName, bool watchFile)
        {
            Log4NetLogWriterFactory.Use(configFileName, watchFile);
        }
    }
}