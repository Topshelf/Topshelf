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
namespace Topshelf.Hosts
{
    using System;
    using System.Threading;
    using log4net;

    public class ConsoleHost
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (ConsoleHost));
        private readonly IServiceCoordinator _coordinator;


        public ConsoleHost(IServiceCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        public void Run()
        {
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

            _coordinator.Start(); //user code starts

            _log.InfoFormat("The service is running, press Control+C to exit.");

            WaitHandle.WaitAny(waitHandles); //will wait until a termination trigger occurs
        }
    }
}