namespace Topshelf.Commands.WinService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Magnum.CommandLineParser;

    public class RunCommand :
        Command
    {
        public string Name
        {
            get; set;
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            var shouldInstall = args.Where(x => x is ITokenElement)
                .Select(x => x as ITokenElement)
                .Where(x => x.Token == "install");

            var shouldUninstall = args.Where(x => x is ITokenElement)
                .Select(x => x as ITokenElement)
                .Where(x => x.Token == "uninstall");

            //some kind of if?
            RunAsService();
        }

        void RunAsService()
        {
            //stuff
        }
    }
}