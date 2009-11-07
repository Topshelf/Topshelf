namespace Topshelf.Commands.WinService.SubCommands
{
    using System;
    using System.Collections.Generic;
    using Hosts;
    using log4net;
    using Magnum.CommandLineParser;

    public class InstallService :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(InstallService));
        public string Name
        {
            get { return "install"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            _log.Info("Received service install notification");

            var fullServiceName = ""; //configuration.WinServiceSettings.FullServiceName
            if (HostServiceInstaller.IsInstalled(fullServiceName))
            {
                string message = string.Format("The {0} service has already been installed.", fullServiceName);
                _log.Error(message);

                return;
            }

            new HostServiceInstaller(configuration)
                .Register();
        }
    }
}