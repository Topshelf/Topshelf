// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Supervise.Commands
{
    using Logging;
    using Runtime;

    public class RestartCommand
    {
        readonly LogWriter _log = HostLogger.Get<RestartCommand>();


        public void Execute(HostControl host, ServiceHandle _service, HostSettings _settings)
        {
//            try
//            {
//                _log.InfoFormat("Restarting service: {0}", _settings.ServiceName);
//
//                var control = new AppDomainHostControl(this);
//                _service.Stop(control);
//
//                UnloadServiceAppDomain();
//
//                _log.DebugFormat("Service AppDomain unloaded: {0}", _settings.ServiceName);
//
//                _service = CreateServiceInAppDomain();
//
//                _log.DebugFormat("Service created in new AppDomain: {0}", _settings.ServiceName);
//
//                _service.Start(control);
//
//                _log.InfoFormat("The service has been restarted: {0}", _settings.ServiceName);
//            }
//            catch (Exception ex)
//            {
//                _log.Error("Failed to restart service", ex);
//                _hostControl.Stop();
//            }
        }
    }
}