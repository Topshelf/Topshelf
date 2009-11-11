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
namespace Topshelf.Commands.WinService.SubCommands
{
    using System.Collections.Generic;
    using Configuration;
    using Configuration.Dsl;
    using Hosts;
    using log4net;
    using Magnum.CommandLineParser;

    public class InstallService :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(InstallService));
        string _fullServiceName;
        WinServiceSettings _settings;

        public InstallService( IRunConfiguration configuration)
        {
            _fullServiceName = configuration.WinServiceSettings.FullServiceName;
            _settings = configuration.WinServiceSettings;
        }

        #region Command Members

        public string Name
        {
            get { return "install"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            _log.Info("Received service install notification");

            if (WinServiceHelper.IsInstalled(_fullServiceName))
            {
                string message = string.Format("The {0} service has already been installed.", _fullServiceName);
                _log.Error(message);

                return;
            }

            WinServiceHelper.Register(_fullServiceName, new HostServiceInstaller(_settings));
        }

        #endregion
    }
}