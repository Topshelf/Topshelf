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
namespace Topshelf
{
    using System;
    using System.IO;
    using Configuration;
    using log4net;

    /// <summary>
    /// Entry point into the Host infrastructure
    /// </summary>
    public static class Runner
    {
        static readonly ILog _log = LogManager.GetLogger(typeof (Runner));

        static Runner()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Fatal("Host encountered an unhandled exception on the AppDomain", (Exception) e.ExceptionObject);
        }

        /// <summary>
        /// Go go gadget
        /// </summary>
        public static void Host(RunConfiguration configuration, string[] args)
        {
            if (args.Length > 0)
                _log.DebugFormat("Command Line Arguments: '{0}'", args);

            var a = TopshelfArgumentParser.Parse(args);
            TopshelfDispatcher.Dispatch(configuration, a);
        }
    }
}