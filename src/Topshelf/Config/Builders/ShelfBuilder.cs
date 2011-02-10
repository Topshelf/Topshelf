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
	using System.Linq;
	using Hosts;
	using log4net;
	using Messages;
	using Model;
	using Stact;


	public class ShelfBuilder :
		RunBuilder
	{
		readonly ILog _log;
		readonly HostChannel _channel;

		public ShelfBuilder(ServiceDescription description, HostChannel channel)
			: base(description)
		{
			_channel = channel;
			_log = LogManager.GetLogger("Topshelf.Shelf." + description.Name);
		}

		public override Host Build()
		{
			if (ServiceBuilders.Count > 1)
				throw new HostConfigurationException("A shelf can only have one service configured");

			ServiceBuilder builder = ServiceBuilders.Single();

			_log.DebugFormat("[Shelf:{0}] Building Service: {1}", Description.Name, builder.Name);

			var controllerFactory = new ServiceControllerFactory();
			ActorFactory<IServiceController> factory = controllerFactory.CreateFactory(inbox =>
				{
					var publish = new PublishChannel(_channel, inbox);

					IServiceController service = builder.Build(inbox, publish);

					return service;
				});

			ActorInstance instance = factory.GetActor();

			_channel.Connect(x => { x.AddChannel(instance); });

			// this creates the state machine instance in the shelf and tells the servicecontroller
			// to create the service
			instance.Send(new CreateService(Description.Name));

			return new ShelfHost(instance);
		}
	}
}