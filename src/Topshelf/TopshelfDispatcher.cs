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
namespace Topshelf
{
    using System.Collections.Generic;
    using System.Linq;
    using Commands;
    using Configuration;
    using log4net;

    public static class TopshelfDispatcher
    {
        static readonly ILog _log = LogManager.GetLogger("Topshelf.TopshelfDispatcher");

        public static void Dispatch(RunConfiguration config, TopshelfArguments args)
        {
            //find the command by the args 'Command'
            var run = new RunCommand(config.Coordinator, config.WinServiceSettings.ServiceName);

			if(!string.IsNullOrEmpty(args.Instance))
            {
                _log.Info("Using instance name from commandline.");
                config.WinServiceSettings.ServiceName = new ServiceName(
                    config.WinServiceSettings.ServiceName.Name,
                    args.Instance);
            }

            var command = new List<Command>
                                  {
                                      run,
                                      new InstallService(config.WinServiceSettings),
                                      new UninstallService(config.WinServiceSettings)
                                  }
                .Where(x => x.Name == args.ActionName)
                .DefaultIfEmpty(run)
                .SingleOrDefault();

            _log.DebugFormat("Running command: '{0}'", command.Name);

            command.Execute();
        }
    }
}