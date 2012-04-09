namespace Topshelf.Host
{
    using System;
    using System.IO;
    using Logging;
#if NET40
    using NancyDashboard;
#endif

    public class Program
    {
        static readonly ILog _log = Logger.Get(ShelfHost.DefaultServiceName);

        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static void Main()
        {
            BootstrapLogger();

            HostFactory.Run(x =>
                {
                    x.BeforeStartingServices(() => Console.WriteLine("[Topshelf] Preparing to start host services"));

                    x.AfterStartingServices(() => Console.WriteLine("[Topshelf] All services have been started"));

                    x.SetServiceName(ShelfHost.DefaultServiceName);
                    x.SetDisplayName(ShelfHost.DefaultServiceName);
                    x.SetDescription("Topshelf Service Host");

                    x.RunAsLocalSystem();

#if NET40
                    x.EnableDashboardWebServices();
#endif

                    x.Service<ShelfHost>(y =>
                        {
                            y.SetServiceName(ShelfHost.DefaultServiceName);
                            y.ConstructUsing((name, coordinator) => new ShelfHost(coordinator));
                            y.WhenStarted(host => host.Start());
                            y.WhenStopped(host => host.Stop());
                        });

                    x.AfterStoppingServices(() => Console.WriteLine("[Topshelf] All services have been stopped"));
                });

            // shutdown log4net just before we exit!
            Logger.Shutdown();
        }

        static void BootstrapLogger()
        {
            string configurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");

            Log4NetLogger.Use(configurationFilePath);

            _log.DebugFormat("Logging configuration loaded: {0}", configurationFilePath);
        }
    }
}