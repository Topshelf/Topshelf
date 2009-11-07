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
    using Configuration.Dsl;
    using Exceptions;
    using Hosts;
    using log4net;
    using Magnum.CommandLineParser;

    public class RunCommand :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(RunCommand));

        #region Command Members

        public string Name
        {
            get { return "run winservice"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            var shouldInstall = args.Where(x => x is ITokenElement)
                .Select(x => x as ITokenElement)
                .Where(x => x.Token == "install");

            var shouldUninstall = args.Where(x => x is ITokenElement)
                .Select(x => x as ITokenElement)
                .Where(x => x.Token == "uninstall");

            //some kind of if?
            RunAsService("full service name");
        }

        #endregion

        void RunAsService(string fullServiceName)
        {
            _log.Info("Received service start notification");

            if (!HostServiceInstaller.IsInstalled(fullServiceName))
            {
                string message = string.Format("The {0} service has not been installed yet. Please run {1} -install.",
                                               fullServiceName, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);
                throw new ConfigurationException(message);
            }

            IRunConfiguration configuration = null;
            var inServiceHost = new ServiceHost(configuration.Coordinator);
            inServiceHost.Run();
        }
    }
}