namespace Topshelf.Commands
{
    using System.Collections.Generic;
    using Magnum.CommandLineParser;

    public interface Command
    {
        string Name { get; }
        void Execute(IEnumerable<ICommandLineElement> args);
    }
}