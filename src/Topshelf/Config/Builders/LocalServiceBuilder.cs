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
	using Exceptions;
	using Model;
	using Stact;


	public class LocalServiceBuilder<T> :
		ServiceBuilder<T>
		where T : class
	{
		readonly Action<T> _continue;
		readonly ServiceDescription _description;
		readonly DescriptionServiceFactory<T> _factory;
		readonly Action<T> _pause;
		readonly Action<T> _start;
		readonly Action<T> _stop;

		public LocalServiceBuilder(ServiceDescription description, string name, DescriptionServiceFactory<T> factory,
		                           Action<T> start, Action<T> stop,
		                           Action<T> pause, Action<T> @continue)
		{
			Name = name;
			_description = description;
			_factory = factory;
			_start = start;
			_stop = stop;
			_pause = pause;
			_continue = @continue;
		}

		public IServiceController Build(Inbox inbox, IServiceChannel coordinatorChannel)
		{
			var serviceController = new LocalServiceController<T>(Name,
			                                                      inbox,
			                                                      coordinatorChannel,
			                                                      StartService,
			                                                      StopService,
			                                                      PauseService,
			                                                      ContinueService,
			                                                      CreateService);

			return serviceController;
		}

		public string Name { get; private set; }

		void StartService(T service)
		{
			try
			{
				_start(service);
			}
			catch (Exception ex)
			{
				throw new ServiceControlException(Name, typeof(T), "Service Start Exception", ex);
			}
		}

		void StopService(T service)
		{
			try
			{
				_stop(service);
			}
			catch (Exception ex)
			{
				throw new ServiceControlException(Name, typeof(T), "Service Stop Exception", ex);
			}
		}

		void PauseService(T service)
		{
			try
			{
				_pause(service);
			}
			catch (Exception ex)
			{
				throw new ServiceControlException(Name, typeof(T), "Service Pause Exception", ex);
			}
		}

		void ContinueService(T service)
		{
			try
			{
				_continue(service);
			}
			catch (Exception ex)
			{
				throw new ServiceControlException(Name, typeof(T), "Service Continue Exception", ex);
			}
		}

		T CreateService(string name, IServiceChannel coordinatorChannel)
		{
			return _factory(_description, name, coordinatorChannel);
		}
	}
}