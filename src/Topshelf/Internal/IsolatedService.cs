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
	using Microsoft.Practices.ServiceLocation;

	public class IsolatedService<TService> :
		MarshalByRefObject,
		IServiceController
	{
		private bool _disposed;
		private TService _instance;
		private IServiceLocator _serviceLocator;

		public IsolatedService()
		{
			State = ServiceState.Stopped;
		}

		public Action<TService> StartAction { get; set; }
		public Action<TService> StopAction { get; set; }
		public Action<TService> PauseAction { get; set; }
		public Action<TService> ContinueAction { get; set; }
		public Func<IServiceLocator> CreateServiceLocator { get; set; }

		public Type ServiceType
		{
			get { return typeof (TService); }
		}

		public IServiceLocator ServiceLocator
		{
			get
			{
				if (_serviceLocator == null)
				{
					_serviceLocator = CreateServiceLocator();
				}

				return _serviceLocator;
			}
		}

		public ServiceState State { get; protected set; }
		public string Name { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Start()
		{
			_instance = ServiceLocator.GetInstance<TService>();
			StartAction(_instance);
			State = ServiceState.Started;
		}

		public void Stop()
		{
            if(_instance != null)
			    StopAction(_instance);
			State = ServiceState.Stopped;
		}

		public void Pause()
		{
			PauseAction(_instance);
			State = ServiceState.Paused;
		}

		public void Continue()
		{
			ContinueAction(_instance);
			State = ServiceState.Started;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing) return;
			if (!_disposed) return;

			StartAction = null;
			StopAction = null;
			PauseAction = null;
			ContinueAction = null;
			CreateServiceLocator = null;
			_serviceLocator = null;
			_instance = default(TService);
			_disposed = true;
		}

		~IsolatedService()
		{
			Dispose(false);
		}
	}
}