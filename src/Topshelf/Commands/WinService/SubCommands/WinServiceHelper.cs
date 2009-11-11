namespace Topshelf.Commands.WinService.SubCommands
{
    using System;
    using System.Collections;
    using System.Configuration.Install;
    using System.Reflection;
    using System.ServiceProcess;
    using Configuration;
    using Hosts;
    using log4net;

    public static class WinServiceHelper
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(WinServiceHelper));

        public static void ConfigureServiceInstaller(ServiceInstaller installer, WinServiceSettings settings)
        {
            installer.ServiceName = settings.FullServiceName;
            installer.Description = settings.Description;
            installer.DisplayName = settings.FullDisplayName;
            installer.ServicesDependedOn = settings.Dependencies.ToArray();
            installer.StartType = ServiceStartMode.Automatic;
        }
        public static void ConfigureServiceProcessInstaller(ServiceProcessInstaller installer, Credentials credentials)
        {
            installer.Username = credentials.Username;
            installer.Password = credentials.Password;
            installer.Account = credentials.AccountType;
        }


        public static void Register(string fullServiceName, HostServiceInstaller installer)
        {
            _log.DebugFormat("Attempting to install {0}", fullServiceName);
            if (!WinServiceHelper.IsInstalled(fullServiceName))
            {
                using (var ti = new TransactedInstaller())
                {
                    ti.Installers.Add(installer);

                    string path = string.Format("/assemblypath={0}", Assembly.GetEntryAssembly().Location);
                    string[] commandLine = { path };

                    var context = new InstallContext(null, commandLine);
                    ti.Context = context;

                    var savedState = new Hashtable();

                    ti.Install(savedState);
                }
            }
            else
            {
                Console.WriteLine("Service is already installed");
                if (_log.IsInfoEnabled)
                    _log.Info("Service is already installed");
            }
        }
        public static void Unregister(string fullServiceName, HostServiceInstaller installer)
        {
            _log.DebugFormat("Attempting to uninstall {0}", fullServiceName);

            if (WinServiceHelper.IsInstalled(fullServiceName))
            {
                using (var ti = new TransactedInstaller())
                {
                    ti.Installers.Add(installer);

                    string path = string.Format("/assemblypath={0}", Assembly.GetEntryAssembly().Location);
                    string[] commandLine = { path };

                    var context = new InstallContext(null, commandLine);
                    ti.Context = context;

                    ti.Uninstall(null);
                }
            }
            else
            {
                Console.WriteLine("Service is not installed");
                if (_log.IsInfoEnabled)
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