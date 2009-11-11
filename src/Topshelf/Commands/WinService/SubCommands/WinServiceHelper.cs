namespace Topshelf.Commands.WinService.SubCommands
{
    using System.ServiceProcess;
    using Configuration;

    public static class WinServiceHelper
    {
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