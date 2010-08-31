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
namespace Topshelf.Commands
{
    using Configuration;
    using log4net;
    using WinService.SubCommands;

    public class InstallService :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof (InstallService));

        readonly WinServiceSettings _settings;

        public InstallService(WinServiceSettings settings)
        {
            _settings = settings;
        }

        #region Command Members

        public ServiceActionNames Name
        {
            get { return ServiceActionNames.Install; }
        }

        public void Execute()
        {
            _log.Info("Received service install notification");

            if (WinServiceHelper.IsInstalled(_settings.ServiceName.FullName))
            {
                string message = string.Format("The {0} service has already been installed.", _settings.ServiceName.FullName);
                _log.Error(message);

                return;
            }

            var installer = new HostServiceInstaller(_settings);
            WinServiceHelper.Register(_settings.ServiceName.FullName, installer);
            if (WinServiceHelper.IsInstalled(_settings.ServiceName.FullName))
            {
              WinServiceHelper.SetRecoveryOptions(_settings.ServiceName.FullName, _settings.RecoveryOptions);
            }
        }

        #endregion
    }
}