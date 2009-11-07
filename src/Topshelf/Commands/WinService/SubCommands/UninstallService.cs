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

        public string Name
        {
            get { return "uninstall service"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            string fullServiceName = "";
            IRunConfiguration configuration = null;

            if (!HostServiceInstaller.IsInstalled(fullServiceName))
            {
                string message = string.Format("The {0} service has not been installed.", fullServiceName);
                _log.Error(message);

                return;
            }

            _log.Info("Received serice uninstall notification");
            new HostServiceInstaller(configuration)
                .Unregister(fullServiceName);
        }
    }
}