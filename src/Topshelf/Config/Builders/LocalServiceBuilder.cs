// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Builders
{
	using System;
	using Topshelf.Model;
	using Stact;


	public class LocalServiceBuilder<T> :
		ServiceBuilder<T>
		where T : class
	{
		readonly InternalServiceFactory<T> _factory;
		readonly string _name;
		readonly Action<T> _start;
		readonly Action<T> _stop;

		public LocalServiceBuilder(string name, InternalServiceFactory<T> factory, Action<T> start, Action<T> stop)
		{
			_name = name;
			_factory = factory;
			_start = start;
			_stop = stop;
		}

		public IServiceController Build(Inbox inbox, IServiceChannel coordinatorChannel)
		{
			var serviceController = new LocalServiceController<T>(_name,
			                                                      inbox,
			                                                      coordinatorChannel,
			                                                      StartService,
			                                                      StopService,
			                                                      PauseService,
			                                                      ContinueService,
			                                                      CreateService);

			return serviceController;
		}

		public string Name
		{
			get { return _name; }
		}

		void StartService(T service)
		{
			_start(service);
		}

		void StopService(T service)
		{
			_stop(service);
		}

		void PauseService(T service)
		{
		}

		void ContinueService(T service)
		{
		}

		T CreateService(string name, IServiceChannel coordinatorChannel)
		{
			return _factory(name, coordinatorChannel);
		}
	}
}