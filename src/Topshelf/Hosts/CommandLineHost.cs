namespace Topshelf.Hosts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;
    using log4net;
    using Model;

    public class CommandLineHost :
        Host
    {
        readonly ILog _log = LogManager.GetLogger(typeof (CommandLineHost));
        readonly IServiceCoordinator _coordinator;
        readonly ServiceName _serviceName;

        public CommandLineHost(ServiceName name, IServiceCoordinator coordinator)
        {
            _serviceName = name;
            _coordinator = coordinator;
        }

        public void Host()
        {
            //TODO: hijacked the host
            //TODO: feels hacky
            var servicesToStart = (IEnumerable<string>)new string[] { "ALL" };

            CheckToSeeIfWinServiceRunning();

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

            _coordinator.Start(); //user code starts



            _log.InfoFormat("The service is running, press Control+C to exit.");

            WaitHandle.WaitAny(waitHandles); //will wait until a termination trigger occurs
        }

        void CheckToSeeIfWinServiceRunning()
        {
            if (ServiceController.GetServices().Where(s => s.ServiceName == _serviceName.FullServiceName).Any())
            {
                _log.WarnFormat("There is an instance of this {0} running as a windows service", _serviceName);
            }
        }
    }
}