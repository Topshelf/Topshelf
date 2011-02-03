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
namespace Topshelf.Dashboard
{
	using System.Collections.Generic;
	using Model;
	using Stact;
	using Stact.ServerFramework;


	public class DashboardConnectionHandler :
		PatternMatchConnectionHandler
	{
		readonly StatusChannel _statusChannel;

		public DashboardConnectionHandler(ServiceCoordinator serviceCoordinator)
			:
				base("^/dashboard", "GET")
		{
			_statusChannel = new StatusChannel(serviceCoordinator);
		}

		protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
		{
			return _statusChannel;
		}


		class StatusChannel :
			Channel<ConnectionContext>
		{
			readonly Fiber _fiber;
			readonly ServiceCoordinator _serviceCoordinator;

			public StatusChannel(ServiceCoordinator serviceCoordinator)
			{
				_serviceCoordinator = serviceCoordinator;
				_fiber = new PoolFiber();
			}

			public void Send(ConnectionContext context)
			{
				_fiber.Add(() =>
					{
						IEnumerable<ServiceInfo> infos = _serviceCoordinator.Status();
						var view = new DashboardView(infos);

						context.Response.RenderSparkView(view, "dashboard.html");
						context.Complete();
					});
			}
		}
	}
}