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
    using System.Collections.Generic;
    using Configuration;
    using Internal;
    using Internal.Actions;
    using log4net;

    /// <summary>
    /// Entry point into the Host infrastructure
    /// </summary>
    public static class Runner
    {
        static readonly IDictionary<NamedAction, IAction> _actions = new Dictionary<NamedAction, IAction>();
        static readonly ILog _log = LogManager.GetLogger(typeof (Runner));

        static Runner()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            _actions.Add(ServiceNamedAction.Install, new InstallServiceAction());
            _actions.Add(ServiceNamedAction.Uninstall, new UninstallServiceAction());
            _actions.Add(NamedAction.Console, new RunAsConsoleAction());
            _actions.Add(ServiceNamedAction.Service, new RunAsServiceAction());
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Fatal("Host encountered an unhandled exception on the AppDomain", (Exception) e.ExceptionObject);
        }

        /// <summary>
        /// Go go gadget
        /// </summary>
        public static void Host(IRunConfiguration configuration, params string[] args)
        {
            _log.Info("Starting Host");
            _log.DebugFormat("Arguments: {0}", string.Join(",", args));

            Parser.Args a = Parser.ParseArgs(args);

            
            
            //smell (one should delegate to the other
            //violation of demeter here
            //args are just that they should not have behaviour
            //how much should configuration no about the darn args though?
            if (a.InstanceName != null) configuration.WinServiceSettings.InstanceName = a.InstanceName;

            //smell?
            NamedAction actionKey = Parser.GetActionKey(a, configuration.DefaultAction);

            IAction action = _actions[actionKey];
            _log.DebugFormat("Running action: '{0}'", action);

            action.Do(configuration);
        }
    }
}