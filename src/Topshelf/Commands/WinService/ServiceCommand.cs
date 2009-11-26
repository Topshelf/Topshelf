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
namespace Topshelf.Commands.WinService
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Exceptions;
    using log4net;
    using Magnum.CommandLineParser;
    using Model;
    using SubCommands;

    public class ServiceCommand :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(ServiceCommand));
        readonly IServiceCoordinator _coordinator;
        private readonly WinServiceSettings _settings;

        public ServiceCommand(IServiceCoordinator coordinator, WinServiceSettings settings)
        {
            _coordinator = coordinator;
            _settings = settings;
        }

        #region Command Members

        public string Name
        {
            get { return "service"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            if(args.Count() == 0)
            {
                RunAsService(_settings.FullServiceName);
                return;
            }

            var subcommand = args.Take(1)
                .Where(x => x is ISwitchElement)
                .Select(x => x as ISwitchElement)
                .Select(x => x.Key)
                .DefaultIfEmpty("")
                .First();

            //processing out the instance argument
            var instance = args
                .Where(x => x is IDefinitionElement)
                .Select(x => x as IDefinitionElement)
                .Where(x => x.Key == "instance")
                .Select(x => x.Value)
                .DefaultIfEmpty("")
                .First();

            //instance override
            _settings.InstanceName = instance;

            var subcommands = new List<Command>
                              {
                                  new InstallService(_settings),
                                  new UninstallService(_settings)
                              };

            var oa = subcommands
                .Where(x => x.Name == subcommand)
                .Single();

            //need to skip two now. ?
            oa.Execute(args.Skip(1).ToList());
        }

        #endregion



        void RunAsService(string fullServiceName)
        {
            _log.Info("Received service start notification");

            if (!WinServiceHelper.IsInstalled(fullServiceName))
            {
                string message = string.Format("The {0} service has not been installed yet. Please run {1} -install.",
                                               fullServiceName, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);
                throw new ConfigurationException(message);
            }

            var inServiceHost = new ServiceHost(_coordinator);
            inServiceHost.Run();
        }
    }
}