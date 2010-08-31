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
    using System.Reflection;
    using Commands.WinService;
    using Commands.WinService.SubCommands;
    using Configuration;
    using Exceptions;
    using log4net;
    using Model;

    public class WinServiceHost :
        Host
    {
        readonly ILog _log = LogManager.GetLogger(typeof (WinServiceHost));
        readonly IServiceCoordinator _coordinator;
        readonly ServiceName _fullServiceName;

        public WinServiceHost(IServiceCoordinator coordinator, ServiceName fullServiceName)
        {
            _coordinator = coordinator;
            _fullServiceName = fullServiceName;
        }

        public void Host()
        {
            _log.Info("Starting up as a winservice application");

            if (!WinServiceHelper.IsInstalled(_fullServiceName.FullName))
            {
                string message =
                    string.Format("The {0} service has not been installed yet. Please run {1} service install.",
                                  _fullServiceName, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);
                throw new ConfigurationException(message);
            }
            var inServiceHost = new ServiceHost(_coordinator);
            inServiceHost.Run();
        }
    }
}