// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Runtime.DotNetCore
{
    using System;
    using HostConfigurators;
    using Logging;


    public class DotNetCoreHostEnvironment :
        HostEnvironment
    {
        readonly LogWriter _log = HostLogger.Get(typeof(DotNetCoreHostEnvironment));
        HostConfigurator _hostConfigurator;

        public DotNetCoreHostEnvironment(HostConfigurator configurator)
        {
            _hostConfigurator = configurator;
        }

        public string CommandLine => CommandLineParser.CommandLine.GetUnparsedCommandLine();

        public bool IsAdministrator => false;

        public bool IsRunningAsAService => false;

        public bool IsServiceInstalled(string serviceName)
        {
            return false;
        }

        public bool IsServiceStopped(string serviceName)
        {
            return false;
        }

        public void StartService(string serviceName, TimeSpan startTimeOut)
        {
            throw new NotImplementedException();
        }

        public void StopService(string serviceName, TimeSpan stopTimeOut)
        {
            throw new NotImplementedException();
        }

        #if !NETCORE
        public void InstallService(InstallHostSettings settings, Action<InstallHostSettings> beforeInstall, Action afterInstall, Action beforeRollback, Action afterRollback)
        {
            throw new NotImplementedException();
        }
        #endif

        public void UninstallService(HostSettings settings, Action beforeUninstall, Action afterUninstall)
        {
            throw new NotImplementedException();
        }

        public bool RunAsAdministrator()
        {
            throw new NotImplementedException();
        }

        public Host CreateServiceHost(HostSettings settings, ServiceHandle serviceHandle)
        {
            throw new NotImplementedException();
        }

        public void SendServiceCommand(string serviceName, int command)
        {
            throw new NotImplementedException();
        }
    }
}