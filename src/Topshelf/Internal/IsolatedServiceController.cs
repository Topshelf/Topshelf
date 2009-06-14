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
	public class IsolatedServiceController<TService> :
		MarshalByRefObject,
		IServiceController where TService : class
	{
	    ServiceController<TService> _serviceController = new ServiceController<TService>();

		
	    public ServiceState State
	    {
	        get { return _serviceController.State; }
	    }

	    public Type ServiceType
	    {
            get { return _serviceController.ServiceType; }
	    }

	    public IServiceLocator ServiceLocator
	    {
	        get { return _serviceController.ServiceLocator; }
	    }


	    public string Name
	    {
	        get { return _serviceController.Name; }
            set { _serviceController.Name = value; }
	    }

	    public void Start()
	    {
	        _serviceController.Start();
	    }

	    public void Stop()
	    {
	        _serviceController.Stop();
	    }

	    public void Pause()
	    {
	        _serviceController.Pause();
	    }

	    public void Continue()
	    {
	        _serviceController.Continue();
	    }

	    public void Dispose()
	    {
	        _serviceController.Dispose();
	    }

	    public Action<TService> StartAction
	    {
	        get { return _serviceController.StartAction; }
	        set { _serviceController.StartAction = value; }
	    }

	    public Action<TService> StopAction
	    {
	        get { return _serviceController.StopAction; }
	        set { _serviceController.StopAction = value; }
	    }

	    public Action<TService> PauseAction
	    {
	        get { return _serviceController.PauseAction; }
	        set { _serviceController.PauseAction = value; }
	    }

	    public Action<TService> ContinueAction
	    {
	        get { return _serviceController.ContinueAction; }
	        set { _serviceController.ContinueAction = value; }
	    }

	    public Func<IServiceLocator> CreateServiceLocator
	    {
	        get { return _serviceController.CreateServiceLocator; }
	        set { _serviceController.CreateServiceLocator = value; }
	    }
	}
}