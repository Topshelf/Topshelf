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
    using log4net;
    using Magnum.CommandLineParser;

    public class InstallService :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(InstallService));

        readonly WinServiceSettings _settings;

        public InstallService(WinServiceSettings settings)
        {
            _settings = settings;
        }

        #region Command Members

        public string Name
        {
            get { return "install"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            _log.Info("Received service install notification");

            if (WinServiceHelper.IsInstalled(_settings.FullServiceName))
            {
                string message = string.Format("The {0} service has already been installed.", _settings.FullServiceName);
                _log.Error(message);

                return;
            }

            WinServiceHelper.Register(_settings.FullServiceName, new HostServiceInstaller(_settings));
        }

        #endregion
    }
}