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
namespace Topshelf.Internal
{
    using System;
    using System.Diagnostics;
    using Microsoft.Practices.ServiceLocation;

    [DebuggerDisplay("Isolated Service({Name}) - {State}")]
    public class FacadeToIsolatedServiceController<TService> :
        IServiceController
        where TService : class
    {
        private AppDomain _domain;
        private ServiceControllerProxy _remoteServiceController;
        private ControllerDelegates<TService> _delegates = new ControllerDelegates<TService>();

        public void Start()
        {
            var settings = AppDomain.CurrentDomain.SetupInformation;

            settings.ShadowCopyFiles = "true";

            if (!string.IsNullOrEmpty(PathToConfigurationFile))
            {
                settings.ConfigurationFile = PathToConfigurationFile;
            }

            if (Args != null)
                settings.AppDomainInitializerArguments = Args;
            if (ConfigureArgsAction != null)
                settings.AppDomainInitializer = ConfigureArgsAction();

            _domain = AppDomain.CreateDomain(typeof(TService).AssemblyQualifiedName, null, settings);

            _remoteServiceController = _domain.CreateInstanceAndUnwrap<ServiceControllerProxy>(typeof(TService));
            if (_remoteServiceController == null)
                throw new ApplicationException("Unable to create service proxy for " + typeof(TService).Name);

            _remoteServiceController.StartAction = _delegates.StartActionObject;
            _remoteServiceController.StopAction = _delegates.StopActionObject;
            _remoteServiceController.CreateServiceLocator = _delegates.CreateServiceLocator;
            _remoteServiceController.PauseAction = _delegates.PauseActionObject;
            _remoteServiceController.ContinueAction = _delegates.ContinueActionObject;
            _remoteServiceController.Name = Name;

            _remoteServiceController.Start();
        }

        //figure out a way to get rid of these?
        public ControllerDelegates<TService> Delegates { get { return _delegates; } }
        public Action<TService> StartAction { set { _delegates.StartAction = value; } }
        public Action<TService> StopAction { set { _delegates.StopAction = value; } }
        public Action<TService> PauseAction { set { _delegates.PauseAction = value; } }
        public Action<TService> ContinueAction { set { _delegates.ContinueAction = value; } }
        public Func<IServiceLocator> CreateServiceLocator { set { _delegates.CreateServiceLocator = value; } }
        public string Name { get; set; }
        public string PathToConfigurationFile { get; set; }
        public string[] Args { get; set; }
        public Func<AppDomainInitializer> ConfigureArgsAction { get; set; }

        public void Stop()
        {
            _remoteServiceController.IfNotNull(x => x.Stop());

            AppDomain.Unload(_domain);
        }

        public void Pause()
        {
            _remoteServiceController.IfNotNull(x => x.Pause());
        }

        public void Continue()
        {
            _remoteServiceController.IfNotNull(x => x.Continue());
        }

        public void Dispose()
        {
            //do nothing?
        }

        public Type ServiceType
        {
            get { return _remoteServiceController.IfNotNull(x => x.ServiceType, typeof(object)); }
        }

        public ServiceState State
        {
            get { return _remoteServiceController.IfNotNull(x => x.State, ServiceState.Stopped); }
        }

        public IServiceLocator ServiceLocator
        {
            get { return _remoteServiceController.ServiceLocator; }
        }

    }
}