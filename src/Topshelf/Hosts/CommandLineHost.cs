// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Topshelf.Hosts
{
    using System;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;
    using Configuration;
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
            CheckToSeeIfWinServiceRunning();

            _log.Debug("Starting up as a console application");

            var externalTriggeredTerminatation = new ManualResetEvent(false);

            var waitHandles = new WaitHandle[] { externalTriggeredTerminatation};

            Console.CancelKeyPress += delegate
                                          {
                                              _log.Info("Control+C detected, exiting.");
                                              _log.Info("Stopping the Topshelf");

                                              _coordinator.Stop(); //user stop
                                              _coordinator.Dispose();
                                              externalTriggeredTerminatation.Set();
                                          };

            _coordinator.Start(); //user code starts


            _log.InfoFormat("Topshelf is running, press Control+C to exit.");

            WaitHandle.WaitAny(waitHandles); //will wait until a termination trigger occurs
        }

        void CheckToSeeIfWinServiceRunning()
        {
            if (ServiceController.GetServices().Where(s => s.ServiceName == _serviceName.FullName).Any())
            {
                _log.WarnFormat("There is an instance of this {0} running as a windows service", _serviceName);
            }
        }
    }
}