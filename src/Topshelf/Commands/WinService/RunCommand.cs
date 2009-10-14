namespace Topshelf.Commands.WinService
{
    using System;
    using System.Collections.Generic;
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
            throw new NotImplementedException();
        }
    }
}