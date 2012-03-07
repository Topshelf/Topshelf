namespace Topshelf.OS
{
    using System;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;
    using Logging;


    public static class OsDetector
    {
        static ILog _log = Logger.Get("OsDetector");

        public static Os DetectOs()
        {
            Os osCommands = new Nix();

            if (Path.DirectorySeparatorChar == '\\')
                osCommands = new Windows();
            
            _log.InfoFormat("Detected the operating system: '{0}'", osCommands.Description);
            return osCommands;
        }
    }

    public class Windows :
        Os
    {
        static ILog _log = Logger.Get("Windows");

        public string Description
        {
            get { return "win"; }
        }

        public void CheckToSeeIfServiceRunning(ServiceDescription description)
        {
            if (ServiceController.GetServices().Where(s => s.ServiceName == description.GetServiceName()).Any())
                _log.WarnFormat("There is an instance of this {0} running as a windows service", description);
        }
    }

    public class Nix :
        Os
    {
        static ILog _log = Logger.Get("Windows");

        public string Description
        {
            get { return "nix"; }
        }

        public void CheckToSeeIfServiceRunning(ServiceDescription description)
        {
            _log.Warn("Nix not detecting, maybe check the pid?");
        }
    }

    public interface Os
    {
        string Description { get; }
        void CheckToSeeIfServiceRunning(ServiceDescription description);
    }
}