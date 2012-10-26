using System;

namespace Topshelf.Runtime.Linux
{
    public class LinuxHostEnvironment
        : HostEnvironment
    {
        public string CommandLine { get; private set; }

        public bool IsAdministrator { get; private set; }
        
        public bool IsRunningAsAService { get; private set; }
        
        public bool IsServiceInstalled(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void StartService(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void StopService(string serviceName)
        {
            throw new NotImplementedException();
        }

        public void InstallService(
            InstallHostSettings settings, Action beforeInstall, Action afterInstall)
        {
            throw new NotImplementedException();
        }

        public static string GenerateUpstartScript(InstallHostSettings settings)
        {
            return "";
        }

        public static ServicePaths GeneratePathsForService(InstallHostSettings settings)
        {
            return new ServicePathsImpl
                {
                };
        }

        public class ServicePathsImpl 
            : ServicePaths
        {
            public string Configuration { get; set; }
            public string WorkingDirectory { get; set; }
        }

        public void UninstallService(HostSettings settings, Action beforeUninstall, Action afterUninstall)
        {
            throw new NotImplementedException();
        }

        public bool RunAsAdministrator()
        {
            throw new NotImplementedException();
        }

        public Host CreateServiceHost(HostSettings settings, ServiceHandle serviceHandle)
        {
            throw new NotImplementedException();
        }
    }

    public interface ServicePaths
    {
        string Configuration { get; }
        string WorkingDirectory { get; }
    }
}