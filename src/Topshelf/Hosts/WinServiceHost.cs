namespace Topshelf.Hosts
{
    using System.Reflection;
    using Commands.WinService;
    using Commands.WinService.SubCommands;
    using Exceptions;
    using log4net;
    using Model;

    public class WinServiceHost :
        Host
    {
        readonly ILog _log = LogManager.GetLogger(typeof (WinServiceHost));
        readonly IServiceCoordinator _coordinator;
        readonly ServiceName _fullServiceName;

        public WinServiceHost(IServiceCoordinator coordinator, ServiceName fullServiceName)
        {
            _coordinator = coordinator;
            _fullServiceName = fullServiceName;
        }

        public void Host()
        {
            _log.Info("Received service start notification");

            if (!WinServiceHelper.IsInstalled(_fullServiceName.FullServiceName))
            {
                string message =
                    string.Format("The {0} service has not been installed yet. Please run {1} service install.",
                                  _fullServiceName, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);
                throw new ConfigurationException(message);
            }
            var inServiceHost = new ServiceHost(_coordinator);
            inServiceHost.Run();
        }
    }
}