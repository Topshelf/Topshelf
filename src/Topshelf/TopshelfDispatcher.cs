namespace Topshelf.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using Commands;
    using Commands.CommandLine;
    using Configuration;
    using log4net;

    public static class TopshelfDispatcher
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(TopshelfDispatcher));
        static List<Command> _commands = new List<Command>()
                                         {
                                             new RunCommand(null) // how to get the service coordinator in here
                                         };

        public static void Dispatch(IRunConfiguration config, TopshelfArguments args)
        {
            //find the command by the args 'Command'
            Command command = _commands.Where(x=> x.Name == args.Command)
                .Single();

            _log.DebugFormat("Running command: '{0}'", command.Name);

            //what to do with the config?

            //flow the args down
            command.Execute(args.CommandArgs);


        }
    }
}