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
    using System;
    using System.Collections;
    using System.Configuration.Install;
    using System.Reflection;
    using System.ServiceProcess;
    using Configuration;
    using log4net;

    public static class WinServiceHelper
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(WinServiceHelper));

        public static Installer[] BuildInstallers(WinServiceSettings settings)
        {
            var result = new Installer[]
                         {
                             ConfigureServiceInstaller(settings),
                             ConfigureServiceProcessInstaller(settings)
                         };
            return result;
        }

        public static ServiceInstaller ConfigureServiceInstaller(WinServiceSettings settings)
        {
            var installer = new ServiceInstaller
                            {
                                ServiceName = settings.ServiceName.FullName,
                                Description = settings.Description,
                                DisplayName = settings.FullDisplayName,
                                ServicesDependedOn = settings.Dependencies.ToArray(),
                                StartType = ServiceStartMode.Automatic
                            };

            return installer;
        }

        public static ServiceProcessInstaller ConfigureServiceProcessInstaller(WinServiceSettings settings)
        {
            var credentials = settings.Credentials;
            var installer = new ServiceProcessInstaller
                            {
                                Username = credentials.Username,
                                Password = credentials.Password,
                                Account = credentials.AccountType
                            };

            return installer;
        }


        public static void Register(string fullServiceName, HostServiceInstaller installer)
        {
            _log.DebugFormat("Attempting to install '{0}'", fullServiceName);
            if (!IsInstalled(fullServiceName))
            {
                using (var ti = new TransactedInstaller())
                {
                    ti.Installers.Add(installer);

                    var assembly = Assembly.GetEntryAssembly();
                    if(assembly == null) throw new Exception("Assembly.GetEntryAssembly() is null for some reason.");

                    string path = string.Format("/assemblypath={0}", assembly.Location);
                    string[] commandLine = {path};

                    var context = new InstallContext(null, commandLine);
                    ti.Context = context;

                    var savedState = new Hashtable();

                    ti.Install(savedState);
                }
            }
            else
            {
                    _log.Info("Service is already installed");
            }
        }

        public static void Unregister(string fullServiceName, HostServiceInstaller installer)
        {
            _log.DebugFormat("Attempting to uninstall '{0}'", fullServiceName);

            if (IsInstalled(fullServiceName))
            {
                using (var ti = new TransactedInstaller())
                {
                    ti.Installers.Add(installer);

                    var assembly = Assembly.GetEntryAssembly();
                    if (assembly == null) throw new Exception("Assembly.GetEntryAssembly() is null for some reason.");

                    string path = string.Format("/assemblypath={0}", assembly.Location);
                    string[] commandLine = { path };

                    var context = new InstallContext(null, commandLine);
                    ti.Context = context;

                    ti.Uninstall(null);
                }
            }
            else
            {
                _log.Info("Service is not installed");
            }
        }

        public static bool IsInstalled(string fullServiceName)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == fullServiceName)
                    return true;
            }

            return false;
        }
    }
}