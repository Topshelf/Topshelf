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
	using System;
	using log4net;
	using Model;
	using Stact;
	using Stact.ServerFramework;


	public class TopshelfDashboard
	{
		static ChannelAdapter _input;
		static HttpServer _server;
		readonly ServiceDescription _description;
		readonly ILog _log = LogManager.GetLogger("Topshelf.Dashboard.TopshelfDashboard");
		readonly int _port;
		readonly IServiceChannel _serviceCoordinator;

		public TopshelfDashboard(ServiceDescription description, IServiceChannel serviceCoordinator)
		{
			_description = description;
			_serviceCoordinator = serviceCoordinator;
			_port = 8483;
		}

		public Uri ServerUri { get; set; }

		public void Start()
		{
			_input = new ChannelAdapter();
			ServerUri = new UriBuilder("http", "localhost", _port, "Topshelf/" + _description.GetServiceName()).Uri;
			_log.InfoFormat("Loading dashboard at Uri: {0}", ServerUri);
			_server = new HttpServer(ServerUri, new PoolFiber(), _input, new PatternMatchConnectionHandler[]
				{
					new VersionConnectionHandler(),
					new StaticResourceHandler(@"\.png$", "Topshelf.Dashboard.images.", "image/png", "GET"),
					new StaticResourceHandler(@"\.css$", "Topshelf.Dashboard.styles.", "text/css", "GET"),
					new DashboardConnectionHandler(_serviceCoordinator)
				});

			_server.Start();
		}

		public void Stop()
		{
			_server.Stop();
		}
	}
}