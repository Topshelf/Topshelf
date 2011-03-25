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
	using System.Net;
	using Magnum.Extensions;
	using Messages;
	using Model;
	using Stact;
	using Stact.MessageHeaders;
	using Stact.ServerFramework;


	public class DashboardConnectionHandler :
		PatternMatchConnectionHandler
	{
		readonly IServiceChannel _serviceCoordinator;

		public DashboardConnectionHandler(IServiceChannel serviceCoordinator)
			: base("^/$", "GET")
		{
			_serviceCoordinator = serviceCoordinator;
		}

		protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
		{
			return new StatusChannel(_serviceCoordinator);
		}


		class StatusChannel :
			Channel<ConnectionContext>
		{
			readonly IServiceChannel _serviceCoordinator;

			public StatusChannel(IServiceChannel serviceCoordinator)
			{
				_serviceCoordinator = serviceCoordinator;
			}

			public void Send(ConnectionContext context)
			{
				AnonymousActor.New(inbox =>
					{
						_serviceCoordinator.Send<Request<ServiceStatus>>(new RequestImpl<ServiceStatus>(inbox, new ServiceStatus()));

						inbox.Receive<Response<ServiceStatus>>(response =>
							{
								var view = new DashboardView(response.Body.Services);

								context.Response.RenderSparkView(view, "dashboard.html");
								context.Complete();
							}, 30.Seconds(), () =>
								{
									context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
									context.Complete();
								});
					});
			}
		}
	}
}