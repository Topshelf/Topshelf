namespace Topshelf.Commands.CommandLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Internal;
    using Internal.Hosts;
    using log4net;
    using Magnum.CommandLineParser;

    public class RunCommand :
        Command
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(RunCommand));
        private readonly IServiceCoordinator _coordinator;

        public RunCommand(IServiceCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        public string Name
        {
            get { return "Run as Command"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            //TODO: feels hacky
            var servicesToStart = args.Where(x => x is IDefinitionElement)
                .Select(x => x as IDefinitionElement)
                .Where(x => x.Key == "service")
                .Select(x => x.Key)
                .DefaultIfEmpty("ALL");

            //all
            var startAll = servicesToStart.Count() == 1 &&
                           servicesToStart.First() == "ALL";
            //not all

            _log.Debug("Starting up as a console application");

            var internalallyTriggeredTermination = new ManualResetEvent(false);
            var externalTriggeredTerminatation = new ManualResetEvent(false);

            var waitHandles = new WaitHandle[] { internalallyTriggeredTermination, externalTriggeredTerminatation };

            _coordinator.Stopped += (() => internalallyTriggeredTermination.Set());

            Console.CancelKeyPress += delegate
            {
                _log.Info("Control+C detected, exiting.");
                _log.Info("Stopping the service");

                _coordinator.Stop(); //user stop
                _coordinator.Dispose();
                externalTriggeredTerminatation.Set();
            };

            //TODO: Feels hacky
            if(startAll)
                _coordinator.Start(); //user code starts
            else
                foreach (var service in servicesToStart)
                    _coordinator.StartService(service);
                
            

            _log.InfoFormat("The service is running, press Control+C to exit.");

            WaitHandle.WaitAny(waitHandles); //will wait until a termination trigger occurs
        }
    }
}