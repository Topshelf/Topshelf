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
namespace Topshelf.Commands
{
    using System.Diagnostics;
    using Configuration;
    using Hosts;
    using log4net;
    using Model;

    public class RunCommand :
        Command
    {
        static readonly ILog _log = LogManager.GetLogger(typeof (RunCommand));
        readonly IServiceCoordinator _coordinator;
        readonly ServiceName _serviceName;

        public RunCommand(IServiceCoordinator coordinator, ServiceName serviceName)
        {
            _coordinator = coordinator;
            _serviceName = serviceName;
        }


        public ServiceActionNames Name
        {
            get { return ServiceActionNames.Run; }
        }

        public void Execute()
        {
            Host host;
            if (Process.GetCurrentProcess().GetParent().ProcessName == "services")
            {
                _log.Debug("Detected that I am running in the windows services");
                host = new WinServiceHost(_coordinator, _serviceName);
            }
            else
            {
                host = new CommandLineHost(_serviceName, _coordinator);
            }

            host.Host();
        }
    }
}