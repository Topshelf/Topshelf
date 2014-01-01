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
namespace Topshelf.Supervise
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using HostConfigurators;
    using Logging;
    using Runtime;

    /// <summary>
    /// The proxy handle into the separate AppDomain for the service. Uniquely identified
    /// by a GUID name so that we can spin up the next instance on a restart/recycle allowing 
    /// the new instance to warm up before actually putting it into traffic.
    /// </summary>
    public class ServiceHandleProxy :
        ServiceHandle
    {
        readonly HostControl _hostControl;
        readonly LogWriter _log = HostLogger.Get<ServiceHandleProxy>();
        readonly ServiceHandle _service;
        readonly ServiceBuilderFactory _serviceBuilderFactory;
        readonly HostSettings _settings;
        AppDomain _appDomain;
        string _appDomainName;

        public ServiceHandleProxy(HostSettings settings, HostControl hostControl, ServiceBuilderFactory serviceBuilderFactory)
        {
            _settings = settings;
            _hostControl = new HostControlProxy(hostControl);
            _serviceBuilderFactory = serviceBuilderFactory;

            _service = CreateServiceInAppDomain();
        }

        public void Dispose()
        {
            UnloadServiceAppDomain();
        }

        bool ServiceHandle.Start(HostControl hostControl)
        {
            return _service.Start(_hostControl);
        }

        bool ServiceHandle.Pause(HostControl hostControl)
        {
            return _service.Pause(_hostControl);
        }

        bool ServiceHandle.Continue(HostControl hostControl)
        {
            return _service.Continue(_hostControl);
        }

        bool ServiceHandle.Stop(HostControl hostControl)
        {
            return _service.Stop(_hostControl);
        }

        public void Shutdown(HostControl hostControl)
        {
            _service.Shutdown(_hostControl);
        }

        public void SessionChanged(HostControl hostControl, SessionChangedArguments arguments)
        {
            _service.SessionChanged(hostControl, arguments);
        }

        public void CustomCommand(HostControl hostControl, int command)
        {
            _service.CustomCommand(hostControl, command);
        }

        void UnloadServiceAppDomain()
        {
            try
            {
                _service.Dispose();
            }
            finally
            {
                try
                {
                    AppDomain.Unload(_appDomain);
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to unload AppDomain", ex);
                }
            }
        }

        ServiceHandle CreateServiceInAppDomain()
        {
            var appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                    LoaderOptimization = LoaderOptimization.MultiDomainHost,
                };

            var permissionSet = new PermissionSet(PermissionState.Unrestricted);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            AppDomain appDomain = null;
            try
            {
                _appDomainName = string.Format("{0}.{1}.{2}", "Topshelf", _settings.Name,
                    Guid.NewGuid().ToString("N"));

                appDomain = AppDomain.CreateDomain(
                    _appDomainName,
                    null,
                    appDomainSetup,
                    permissionSet,
                    null);

                return CreateServiceHandle(appDomain);
            }
            catch (Exception)
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                    appDomain = null;
                }

                throw;
            }
            finally
            {
                if (appDomain != null)
                    _appDomain = appDomain;
            }
        }

        ServiceHandle CreateServiceHandle(AppDomain appDomain)
        {
            Type type = typeof(ServiceHandleStub);
            string assemblyName = type.Assembly.FullName;
            string typeName = type.FullName ?? typeof(ServiceHandleStub).Name;

            var serviceHandle = (ServiceHandleStub)appDomain.CreateInstanceAndUnwrap(assemblyName, typeName);

            serviceHandle.Create(_serviceBuilderFactory, _settings, HostLogger.CurrentHostLoggerConfigurator);

            return serviceHandle;
        }
    }
}