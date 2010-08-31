// Copyright 2007-2008 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//   http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Commands.WinService
{
    using System;
    using System.ServiceProcess;
    using log4net;
    using Model;

    public class ServiceHost :
        ServiceBase //TODO: Is this what you would InstallUtil?
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (ServiceHost));
        private readonly IServiceCoordinator _coordinator;


        public ServiceHost(IServiceCoordinator coordinator)
        {
            _coordinator = coordinator;
        }

        public void Run()
        {
            _log.Debug("Starting up as a windows service application");
            var servicesToRun = new ServiceBase[] {this};

            Run(servicesToRun);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("Received service start notification");
                _log.DebugFormat("Arguments: {0}", string.Join(",", args));

                _coordinator.Start();
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _log.Info("Received service stop notification");

                _coordinator.Stop();
                _coordinator.Dispose();
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                throw;
            }
        }
    }
}