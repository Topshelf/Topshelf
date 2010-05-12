// Copyright 2007-2008 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Commands.CommandLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;
    using log4net;
    using Magnum.CommandLineParser;
    using Model;

    public class RunCommand :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(RunCommand));
        readonly IServiceCoordinator _coordinator;
        string _serviceName;

        public RunCommand(IServiceCoordinator coordinator, string serviceName)
        {
            _coordinator = coordinator;
            _serviceName = serviceName;
        }

        #region Command Members

        public string Name
        {
            get { return "command"; }
        }

        public void Execute(IEnumerable<ICommandLineElement> args)
        {
            //TODO: hijacked the host
            //TODO: feels hacky
            var servicesToStart = (IEnumerable<string>)new string[]{"ALL"};

            CheckToSeeIfWinServiceRunning();

            if(args != null) // this is start all
                servicesToStart = args.Where(x => x is IDefinitionElement)
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

            var waitHandles = new WaitHandle[] {internalallyTriggeredTermination, externalTriggeredTerminatation};

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
            if (startAll)
                _coordinator.Start(); //user code starts
            else
                foreach (var service in servicesToStart)
                    _coordinator.StartService(service);


            _log.InfoFormat("The service is running, press Control+C to exit.");

            WaitHandle.WaitAny(waitHandles); //will wait until a termination trigger occurs
        }

        void CheckToSeeIfWinServiceRunning()
        {
            if(ServiceController.GetServices().Where(s => s.ServiceName == _serviceName).Any())
            {
                _log.WarnFormat("There is an instance of this {0} running as a windows service", _serviceName);
            }
        }

        #endregion
    }
}