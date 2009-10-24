namespace Topshelf.Internal
{
    using System.Collections.Generic;
    using Magnum.CommandLineParser;

    public class TopshelfArguments
    {
        // Actions
        //   run as console
        //   run as service
        //     install
        //     uninstall
        //   plugin actions
        //     run as winform
        //     run as GTK#
        // instance name
        // services to start?


        // ts service /install /instance=bob

        public string Command { get; set; }
        public IEnumerable<ICommandLineElement> CommandArgs { get; set; }
    }
}