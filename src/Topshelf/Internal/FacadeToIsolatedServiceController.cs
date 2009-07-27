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
		private IsolatedServiceController<TService> _remoteServiceController;

		public void Start()
		{
			var settings = AppDomain.CurrentDomain.SetupInformation;
			settings.ShadowCopyFiles = "true";

            if (!string.IsNullOrEmpty(PathToConfigurationFile))
            {
                settings.ConfigurationFile = PathToConfigurationFile;
            }

			_domain = AppDomain.CreateDomain(typeof (TService).AssemblyQualifiedName, null, settings);

			_remoteServiceController = _domain.CreateInstanceAndUnwrap<IsolatedServiceController<TService>>();
			if (_remoteServiceController == null)
				throw new ApplicationException("Unable to create service proxy for " + typeof (TService).Name);

			_remoteServiceController.CreateServiceLocator = CreateServiceLocator;
			_remoteServiceController.StartAction = StartAction;
			_remoteServiceController.StopAction = StopAction;
			_remoteServiceController.PauseAction = PauseAction;
			_remoteServiceController.ContinueAction = ContinueAction;
			_remoteServiceController.Name = Name;

			_remoteServiceController.Start();
		}

        //figure out a way to get rid of these?
        public Action<TService> StartAction { get; set; }
        public Action<TService> StopAction { get; set; }
        public Action<TService> PauseAction { get; set; }
        public Action<TService> ContinueAction { get; set; }
        public Func<IServiceLocator> CreateServiceLocator { get; set; }
        public string Name { get; set; }
        public string PathToConfigurationFile { get; set; }

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
            get { return _remoteServiceController.IfNotNull(x=>x.ServiceType, typeof(object)); }
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