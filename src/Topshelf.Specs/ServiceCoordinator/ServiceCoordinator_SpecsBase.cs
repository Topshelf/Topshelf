// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Topshelf.Specs.ServiceCoordinator
{
	using System;
	using Magnum.Extensions;
	using Magnum.TestFramework;
	using Model;
	using Stact;


	[Scenario]
	public class ServiceCoordinator_SpecsBase
	{
		public ServiceCoordinator Coordinator { get; set; }

		void Ignored(IServiceCoordinator coordinator)
		{
		}

		protected void CreateService<T>(string serviceName, Action<T> startAction, Action<T> stopAction, Action<T> pauseAction,
		                                Action<T> continueAction, InternalServiceFactory<T> serviceFactory)
			where T : class
		{
			Coordinator.CreateService(serviceName, (inbox, coordinator) => new LocalServiceController<T>(serviceName,
			                                                                                             inbox,
			                                                                                             Coordinator,
			                                                                                             startAction,
			                                                                                             stopAction,
			                                                                                             pauseAction,
			                                                                                             continueAction,
			                                                                                             serviceFactory));
		}

		[Given]
		public void A_service_coordinator()
		{
			Coordinator = new ServiceCoordinator(new PoolFiber(), Ignored, Ignored, Ignored, 10.Seconds());
		}

		[After]
		public void CleanUp()
		{
			Coordinator.Dispose();
			Coordinator = null;
		}
	}
}