namespace Topshelf
{
    using System.Collections.Generic;
    using System.Linq;
    using Magnum.CommandLineParser;

    public static class TopshelfArgumentParser
    {
        public static TopshelfArguments Parse(string commandLine)
        {
            var result = new TopshelfArguments();

            Set(result, P(commandLine));

            return result;
        }

        static void Set(TopshelfArguments args, IEnumerable<ICommandLineElement> commandLineElements)
        {
            var command = commandLineElements
                .Take(1)
                .Select(x => (IArgumentElement)x)
                .Select(x => x.Id)
                .DefaultIfEmpty("commandline")
                .SingleOrDefault();


            args.Command = command;
            //leftovers
            args.CommandArgs = commandLineElements.Skip(1).ToList();
        }

        static IEnumerable<ICommandLineElement> P(string commandLine)
        {
            var parser = new MonadicCommandLineParser();

            return parser.Parse(commandLine);
        }
    }
}