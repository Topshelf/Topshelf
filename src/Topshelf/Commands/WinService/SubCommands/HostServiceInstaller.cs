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
    using System.Collections;
    using System.Configuration.Install;
    using System.ServiceProcess;
    using Configuration;
    using log4net;
    using Microsoft.Win32;

    public class HostServiceInstaller :
        Installer
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(HostServiceInstaller));
        readonly ServiceInstaller _serviceInstaller = new ServiceInstaller();
        readonly ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();
        readonly WinServiceSettings _settings;


        public HostServiceInstaller(WinServiceSettings settings)
        {
            _settings = settings;

            WinServiceHelper.ConfigureServiceInstaller(_serviceInstaller, settings);
            WinServiceHelper.ConfigureServiceProcessInstaller(_serviceProcessInstaller, _settings.Credentials);

            Installers.AddRange(new Installer[] {_serviceProcessInstaller, _serviceInstaller});
        }


        /// <summary>
        /// For the .Net service install infrastructure
        /// </summary>
        /// <param name="stateSaver"></param>
        public override void Install(IDictionary stateSaver)
        {
            if (_log.IsInfoEnabled)
                _log.InfoFormat("Installing Service {0}", _serviceInstaller.ServiceName);

            base.Install(stateSaver);

            if (_log.IsDebugEnabled) _log.Debug("Opening Registry");

            using (RegistryKey system = Registry.LocalMachine.OpenSubKey("System"))
            using (RegistryKey currentControlSet = system.OpenSubKey("CurrentControlSet"))
            using (RegistryKey services = currentControlSet.OpenSubKey("Services"))
            using (RegistryKey service = services.OpenSubKey(_serviceInstaller.ServiceName, true))
            {
                service.SetValue("Description", _serviceInstaller.Description);

                var imagePath = (string) service.GetValue("ImagePath");

                _log.DebugFormat("Service Path {0}", imagePath);

                imagePath += _settings.InstanceName == null ?
                                                                " -service" : " -service -instance:{0}".FormatWith(_settings.InstanceName);

                _log.DebugFormat("ImagePath '{0}'", imagePath);

                service.SetValue("ImagePath", imagePath);
            }

            if (_log.IsDebugEnabled) _log.Debug("Closing Registry");
        }
    }
}