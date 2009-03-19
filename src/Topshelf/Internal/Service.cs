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
	public class Service<TService> :
		ServiceBase<TService>,
		IService
	{
		private TService _instance;

		public Service()
		{
			State = ServiceState.Stopped;
		}

		public void Start()
		{
			_instance = ServiceLocator.GetInstance<TService>();
			StartAction(_instance);
			State = ServiceState.Started;
		}

		public void Stop()
		{
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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && _instance != null )
				_instance = default(TService);
		}
	}
}