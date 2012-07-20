// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.Collections;
    using System.Configuration.Install;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;

    public class HostServiceInstaller :
        IDisposable
    {
        readonly Installer _installer;
        readonly TransactedInstaller _transactedInstaller;

        public HostServiceInstaller(InstallServiceSettings settings)
        {
            _installer = CreateInstaller(settings);

            _transactedInstaller = CreateTransactedInstaller(_installer);
        }

        public HostServiceInstaller(HostSettings settings)
        {
            _installer = CreateInstaller(settings);

            _transactedInstaller = CreateTransactedInstaller(_installer);
        }

        public void Dispose()
        {
            try
            {
                _transactedInstaller.Dispose();
            }
            finally
            {
                _installer.Dispose();
            }
        }

        public void InstallService(Action<InstallEventArgs> beforeInstall, Action<InstallEventArgs> afterInstall)
        {
            if (beforeInstall != null)
                _installer.BeforeInstall += (sender, args) => beforeInstall(args);
            if (afterInstall != null)
                _installer.AfterInstall += (sender, args) => afterInstall(args);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _transactedInstaller.Install(new Hashtable());
        }

        public void UninstallService(Action<InstallEventArgs> beforeUninstall, Action<InstallEventArgs> afterUninstall)
        {
            if (beforeUninstall != null)
                _installer.BeforeUninstall += (sender, args) => beforeUninstall(args);
            if (afterUninstall != null)
                _installer.AfterUninstall += (sender, args) => afterUninstall(args);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _transactedInstaller.Uninstall(null);
        }

        static Installer CreateInstaller(InstallServiceSettings settings)
        {
            var installers = new Installer[]
                {
                    ConfigureServiceInstaller(settings, settings.Dependencies, settings.StartMode),
                    ConfigureServiceProcessInstaller(settings.Account, settings.Username, settings.Password)
                };

            string arguments = " ";

            if (!string.IsNullOrEmpty(settings.InstanceName))
                arguments += string.Format(" -instance \"{0}\"", settings.InstanceName);

            if (!string.IsNullOrEmpty(settings.DisplayName))
                arguments += string.Format(" -displayname \"{0}\"", settings.DisplayName);

            if (!string.IsNullOrEmpty(settings.Name))
                arguments += string.Format(" -servicename \"{0}\"", settings.Name);

            return new HostInstaller(settings, arguments, installers);
        }

        Installer CreateInstaller(HostSettings settings)
        {
            var installers = new Installer[]
                {
                    ConfigureServiceInstaller(settings, new string[] {}, ServiceStartMode.Automatic),
                    ConfigureServiceProcessInstaller(ServiceAccount.LocalService, "", ""),
                };

            string arguments = " ";

            if (!string.IsNullOrEmpty(settings.InstanceName))
                arguments += string.Format(" -instance \"{0}\"", settings.InstanceName);

            if (!string.IsNullOrEmpty(settings.DisplayName))
                arguments += string.Format(" -displayname \"{0}\"", settings.DisplayName);

            if (!string.IsNullOrEmpty(settings.Name))
                arguments += string.Format(" -servicename \"{0}\"", settings.Name);

            return new HostInstaller(settings, arguments, installers);
        }

        static TransactedInstaller CreateTransactedInstaller(Installer installer)
        {
            var transactedInstaller = new TransactedInstaller();

            transactedInstaller.Installers.Add(installer);

            Assembly assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
                throw new TopshelfException("Assembly.GetEntryAssembly() is null for some reason.");

            string path = string.Format("/assemblypath={0}", assembly.Location);
            string[] commandLine = {path};

            var context = new InstallContext(null, commandLine);
            transactedInstaller.Context = context;

            return transactedInstaller;
        }

        static ServiceInstaller ConfigureServiceInstaller(HostSettings settings, string[] dependencies,
            ServiceStartMode startMode)
        {
            var installer = new ServiceInstaller
                {
                    ServiceName = settings.ServiceName,
                    Description = settings.Description,
                    DisplayName = settings.DisplayName,
                    ServicesDependedOn = dependencies,
                    StartType = startMode,
                };

            return installer;
        }

        static ServiceProcessInstaller ConfigureServiceProcessInstaller(ServiceAccount account, string username,
            string password)
        {
            var installer = new ServiceProcessInstaller
                {
                    Username = username,
                    Password = password,
                    Account = account,
                };

            return installer;
        }
    }
}