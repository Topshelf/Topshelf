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

	[Serializable]
	public class FacadeToIsolatedService<TService> :
		ServiceControllerBase<TService>,
		IServiceController
	{
		private AppDomain _domain;
		private IsolatedService<TService> _remoteService;

		public void Start()
		{
			var settings = AppDomain.CurrentDomain.SetupInformation;
			settings.ShadowCopyFiles = "true";
			_domain = AppDomain.CreateDomain(typeof (TService).AssemblyQualifiedName, null, settings);

			_remoteService = _domain.CreateInstanceAndUnwrap<IsolatedService<TService>>();
			if (_remoteService == null)
				throw new ApplicationException("Unable to create service proxy for " + typeof (TService).Name);

			_remoteService.CreateServiceLocator = CreateServiceLocator;
			_remoteService.StartAction = StartAction;
			_remoteService.StopAction = StopAction;
			_remoteService.PauseAction = PauseAction;
			_remoteService.ContinueAction = ContinueAction;
			_remoteService.Name = Name;

			_remoteService.Start();
		}

		public void Stop()
		{
			_remoteService.IfNotNull(x => x.Stop());

			AppDomain.Unload(_domain);
		}

		public void Pause()
		{
			_remoteService.IfNotNull(x => x.Pause());
		}

		public void Continue()
		{
			_remoteService.IfNotNull(x => x.Continue());
		}
	}
}