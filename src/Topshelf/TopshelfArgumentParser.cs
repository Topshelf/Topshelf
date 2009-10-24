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
            var command = commandLineElements.Where(x => x is IArgumentElement)
                .Select(x => (IArgumentElement)x)
                .DefaultIfEmpty(new ArgumentElement("commandline"))
                .Select(x => x.Id)
                .SingleOrDefault();

            args.Command = command;


            //services to start
            var servicesToStart = commandLineElements.Where(x => x is IDefinitionElement)
                .Select(x => x as IDefinitionElement)
                .Where(x => x.Key == "start")
                .Select(x => x.Value)
                .DefaultIfEmpty("ALL");



        }

        static IEnumerable<ICommandLineElement> P(string commandLine)
        {
            var parser = new MonadicCommandLineParser();

            return parser.Parse(commandLine);
        }
    }
}