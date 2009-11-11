namespace Topshelf.Commands.WinService.SubCommands
{
    using System;
    using System.Collections.Generic;
    using Configuration.Dsl;
    using Hosts;
    using log4net;
    using Magnum.CommandLineParser;

    public class UninstallService :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(UninstallService));
        readonly string _fullServiceName = "";
        readonly IRunConfiguration _configuration = null;

        public UninstallService(IRunConfiguration configuration)
        {
            this._fullServiceName = configuration.WinServiceSettings.FullServiceName;
            this._configuration = configuration;
        }

        public string Name
        {
            get { return "uninstall service"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {

            if (!WinServiceHelper.IsInstalled(_fullServiceName))
            {
                string message = string.Format("The {0} service has not been installed.", _fullServiceName);
                _log.Error(message);

                return;
            }

            _log.Info("Received serice uninstall notification");
            WinServiceHelper.Unregister(_fullServiceName, new HostServiceInstaller(_configuration.WinServiceSettings));
        }
    }
}