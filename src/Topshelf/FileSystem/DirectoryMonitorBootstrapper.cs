namespace Topshelf.FileSystem
{
    using System;
    using System.IO;
    using Configuration.Dsl;
    using Shelving;

    public class DirectoryMonitorBootstrapper :
        Bootstrapper<DirectoryMonitor>
    {
        public void InitializeHostedService(IServiceConfigurator<DirectoryMonitor> cfg)
        {
            cfg.HowToBuildService(sb => new DirectoryMonitor(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services")));

            cfg.WhenStarted(a => a.Start());
            cfg.WhenStopped(a => a.Stop());
        }
    }
}