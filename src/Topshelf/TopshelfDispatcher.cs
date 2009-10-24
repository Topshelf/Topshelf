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
    using Configuration.Dsl;
    using log4net;

    public static class TopshelfDispatcher
    {
        static readonly List<Command> _commands = new List<Command>
                                                  {
                                                      new RunCommand(null) // how to get the service coordinator in here
                                                  };

        static readonly ILog _log = LogManager.GetLogger(typeof(TopshelfDispatcher));

        public static void Dispatch(IRunConfiguration config, TopshelfArguments args)
        {
            //find the command by the args 'Command'
            Command command = _commands.Where(x => x.Name == args.Command)
                .Single();

            _log.DebugFormat("Running command: '{0}'", command.Name);

            //what to do with the config?

            //flow the args down
            command.Execute(args.CommandArgs);
        }
    }
}