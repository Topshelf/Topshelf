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
    using System.ServiceProcess;
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
        readonly IServiceCoordinator _coordinator = null;
        readonly RunConfiguration _config;

        public ServiceCommand(IServiceCoordinator coordinator)
        {
            this._coordinator = coordinator;
        }

        #region Command Members

        public string Name
        {
            get { return "service"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            var subcommand = args.Take(1)
                .Where(x => x is ITokenElement)
                .Select(x => x as ITokenElement)
                .Select(x => x.Token)
                .DefaultIfEmpty("")
                .First();

            
            var subcommands = new List<Command>
                              {
                                  new InstallService(_config),
                                  new UninstallService(_config)
                              };

            var oa = subcommands
                .DefaultIfEmpty(this)
                .Where(x => x.Name == subcommand)
                .FirstOrDefault();

            if (oa == this) RunAsService("full service name");
            else oa.Execute(args.Skip(1));
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