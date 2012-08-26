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
    using Topshelf.Builders;
    using Topshelf.HostConfigurators;
    using Topshelf.Runtime;

    public class RehabServiceBuilder<T> :
        ServiceBuilder
        where T : class
    {
        readonly ServiceBuilderFactory _serviceBuilderFactory;

        public RehabServiceBuilder(ServiceBuilderFactory serviceBuilderFactory)
        {
            _serviceBuilderFactory = serviceBuilderFactory;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                ServiceHandle handle = CreateServiceInAppDomain(settings);

                return handle;
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        ServiceHandle CreateServiceInAppDomain(HostSettings settings)
        {
            var appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                    LoaderOptimization = LoaderOptimization.MultiDomainHost
                };

            var permissionSet = new PermissionSet(PermissionState.Unrestricted);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            AppDomain appDomain = null;
            try
            {
                appDomain = AppDomain.CreateDomain(
                    "Topshelf." + settings.Name,
                    null,
                    appDomainSetup,
                    permissionSet,
                    null);

                ServiceHandle appDomainServiceLoader = CreateServiceHandle(appDomain, settings);

                return appDomainServiceLoader;
            }
            catch (Exception ex)
            {
                if (appDomain != null)
                {
                    AppDomain.Unload(appDomain);
                }

                throw;
            }
        }


        ServiceHandle CreateServiceHandle(AppDomain appDomain, HostSettings settings)
        {
            Type type = typeof(AppDomainServiceHandle);
            string assemblyName = type.Assembly.FullName;
            string typeName = type.FullName ?? typeof(AppDomainServiceHandle).Name;
            var loader = (AppDomainServiceHandle)appDomain.CreateInstanceAndUnwrap(assemblyName, typeName);

            loader.Create(_serviceBuilderFactory, settings);

            return new RehabServiceHandle<T>(appDomain, loader);
        }
    }
}