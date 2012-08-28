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
namespace Topshelf.Rehab
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using HostConfigurators;
    using Logging;
    using Runtime;

    public class RehabServiceHandle<T> :
        ServiceHandle,
        HostControl
        where T : class
    {
        readonly LogWriter _log = HostLogger.Get<RehabServiceHandle<T>>();
        readonly ServiceBuilderFactory _serviceBuilderFactory;
        readonly HostSettings _settings;
        AppDomain _appDomain;
        HostControl _hostControl;
        ServiceHandle _service;

        public RehabServiceHandle(HostSettings settings, ServiceBuilderFactory serviceBuilderFactory)
        {
            _settings = settings;
            _serviceBuilderFactory = serviceBuilderFactory;

            _service = CreateServiceInAppDomain();
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            if (_hostControl == null)
                throw new InvalidOperationException("The HostControl reference has not been set, this is invalid");

            _hostControl.RequestAdditionalTime(timeRemaining);
        }

        void HostControl.Stop()
        {
            if (_hostControl == null)
                throw new InvalidOperationException("The HostControl reference has not been set, this is invalid");

            _hostControl.Stop();
        }

        void HostControl.Restart()
        {
            ThreadPool.QueueUserWorkItem(RestartService);
        }

        void IDisposable.Dispose()
        {
            UnloadServiceAppDomain();
        }

        bool ServiceHandle.Start(HostControl hostControl)
        {
            _hostControl = hostControl;
            var control = new AppDomainHostControl(this);
            return _service.Start(control);
        }

        bool ServiceHandle.Pause(HostControl hostControl)
        {
            var control = new AppDomainHostControl(this);
            return _service.Pause(control);
        }

        bool ServiceHandle.Continue(HostControl hostControl)
        {
            var control = new AppDomainHostControl(this);
            return _service.Continue(control);
        }

        bool ServiceHandle.Stop(HostControl hostControl)
        {
            var control = new AppDomainHostControl(this);
            return _service.Stop(control);
        }

        public void Shutdown(HostControl hostControl)
        {
            var control = new AppDomainHostControl(this);
            _service.Shutdown(control);
        }

        void RestartService(object state)
        {
            try
            {
                _log.InfoFormat("Restarting service: {0}", _settings.ServiceName);

                var control = new AppDomainHostControl(this);
                _service.Stop(control);

                UnloadServiceAppDomain();

                _log.DebugFormat("Service AppDomain unloaded: {0}", _settings.ServiceName);

                _service = CreateServiceInAppDomain();

                _log.DebugFormat("Service created in new AppDomain: {0}", _settings.ServiceName);

                _service.Start(control);

                _log.InfoFormat("The service has been restarted: {0}", _settings.ServiceName);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to restart service", ex);
                _hostControl.Stop();
            }
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
                appDomain = AppDomain.CreateDomain(
                    "Topshelf." + _settings.Name,
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
            Type type = typeof(AppDomainServiceHandle);
            string assemblyName = type.Assembly.FullName;
            string typeName = type.FullName ?? typeof(AppDomainServiceHandle).Name;

            var serviceHandle = (AppDomainServiceHandle)appDomain.CreateInstanceAndUnwrap(assemblyName, typeName);

            serviceHandle.Create(_serviceBuilderFactory, _settings, HostLogger.CurrentHostLoggerConfigurator);

            return serviceHandle;
        }
    }
}