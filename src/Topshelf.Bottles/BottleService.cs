namespace Topshelf.Bottles
{
    using System;
    using System.Configuration;
    using System.IO;
    using Configuration.Dsl;
    using Magnum.FileSystem;
    using Shelving;
    using Directory = Magnum.FileSystem.Directory;

    public class BottleService
    {
        BottleWatcher _watcher;
        IDisposable _cleanup;
        FileSystem _fs;

        public void Start()
        {
            //TODO: how to find the services dir
            //TODO: how to get the bottles dir
            //TODO: do we need a custom config?
            string baseDir = ConfigurationManager.AppSettings["BottlesDirectory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bottles");
            _fs = new DotNetFileSystem();
            var bottlesDir = _fs.GetDirectory(baseDir);
            _watcher = new BottleWatcher();
            _cleanup = _watcher.Watch(bottlesDir.Name.GetPath(), CopyToServices);
        }

        void CopyToServices(Directory obj)
        {
            var serviceName = obj.Name.GetName();
            var targetDir = _fs.GetDirectory("Services").GetChildDirectory(serviceName);
            obj.CopyTo(targetDir.Name);
        }

        public void Stop()
        {
            _watcher = null;
            _cleanup.Dispose();
        }
    }

    public class BottleServiceBootstrapper :
        Bootstrapper<BottleService>
    {
        public void InitializeHostedService(IServiceConfigurator<BottleService> cfg)
        {
            cfg.HowToBuildService(name=> new BottleService());
            cfg.WhenStarted(s=>s.Start());
            cfg.WhenStopped(s=>s.Stop());
        }
    }
}