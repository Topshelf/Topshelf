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
    using System.Collections.Generic;
    using System.Linq;
    using Commands;
    using Commands.CommandLine;
    using Commands.WinService;
    using Configuration;
    using Hosts;
    using log4net;

    public static class TopshelfDispatcher
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(TopshelfDispatcher));
        static HostFactory _factory = new AHostFactory();

        public static void Dispatch(RunConfiguration config, TopshelfArguments args)
        {
            //find the command by the args 'Command'
//            var run = new RunCommand(config.Coordinator, config.WinServiceSettings.ServiceName);
//            Command command = new List<Command>
//                              {
//                                  run,
//                                  new ServiceCommand(config.Coordinator, config.WinServiceSettings)
//                              }
//                .Where(x => x.Name == args.Command)
//                .DefaultIfEmpty(run)
//                .SingleOrDefault();
//
//            _log.DebugFormat("Running command: '{0}'", command.Name);
//
            //what to do with the config?
//
            //flow the args down
//            command.Execute(args.CommandArgs);


            //so now my 3 actions or host/install/uninstall
            //host
            _factory.CreateHost(new ServiceName("","")).Host();

            //or install/uninstall
        }
    }
}