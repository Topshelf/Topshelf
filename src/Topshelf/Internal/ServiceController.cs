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
    using System.Runtime.Serialization;

    public class ServiceController<TService> :
		ServiceControllerBase<TService>,
		IServiceController
	{
		private TService _instance;

		public ServiceController()
		{
			State = ServiceState.Stopped;
		}

		public void Start()
		{
			_instance = ServiceLocator.GetInstance<TService>(Name);
            if (_instance == null) throw new CouldntFindServiceException(Name, typeof(TService));
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

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && _instance != null )
				_instance = default(TService);
		}
	}

    public class CouldntFindServiceException : 
        Exception
    {
        public CouldntFindServiceException(string name) : base(string.Format("Couldn't find '{0}' in the ServiceLocator", name))
        {
            
        }

        public CouldntFindServiceException(string name, Type serviceType)
            : base(string.Format("Couldn't find service '{0}' named '{1}' in the ServiceLocator", serviceType.Name, name))
        {

        }

        public CouldntFindServiceException()
        {
        }

        public CouldntFindServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldntFindServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}