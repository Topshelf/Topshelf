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
namespace Topshelf.Shelving
{
	using System;
	using System.Linq;
	using System.Threading;
	using Magnum;
	using Magnum.Channels;
	using Magnum.Collections;
	using Magnum.Extensions;
	using Magnum.Fibers;
	using Messages;


	public class ShelfServiceController :
		IShelfServiceController
	{
		readonly Cache<string, ShelfService> _serviceCache;
		readonly AutoResetEvent _updated = new AutoResetEvent(true);
		UntypedChannel _controllerChannel;
		ChannelConnection _controllerChannelConnection;

		bool _disposed;
		ThreadPoolFiber _fiber;
		bool _stopping;

		public ShelfServiceController()
		{
			_fiber = new ThreadPoolFiber();

			_controllerChannel = new ChannelAdapter();

			_serviceCache = new Cache<string, ShelfService>(key => new ShelfService(key, _controllerChannel));

			_controllerChannelConnection = _controllerChannel.Connect(x =>
				{
					x.AddConsumersFor<ShelfService>()
						.BindUsing<ShelfServiceBinding, string>()
						.ExecuteOnThreadPoolFiber()
						.CreateNewInstanceBy(name => new ShelfService(name, _controllerChannel))
						.PersistInMemoryUsing(_serviceCache);


					x.AddConsumerOf<ServiceStopped>()
						.UsingConsumer(OnServiceStopped)
						.ExecuteOnFiber(_fiber);

					x.ReceiveFromWcfChannel(WellknownAddresses.ShelfServiceCoordinatorAddress,
					                        WellknownAddresses.ShelfServiceCoordinatorPipeName);
				});
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void CreateShelfService(string serviceName, Type bootstrapperType)
		{
			var command = new CreateShelfService
				{
					ServiceName = serviceName,
					ShelfType = ShelfType.Internal,
					BootstrapperType = bootstrapperType,
				};

			_controllerChannel.Send(command);
		}

		public void CreateShelfService(string serviceName)
		{
			var command = new CreateShelfService
				{
					ServiceName = serviceName,
					ShelfType = ShelfType.Folder,
				};

			_controllerChannel.Send(command);
		}

		~ShelfServiceController()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				StopServices(10.Minutes());

				if (_controllerChannelConnection != null)
				{
					_controllerChannelConnection.Dispose();
					_controllerChannelConnection = null;
				}

				_controllerChannel = null;
			}

			_disposed = true;
		}

		void OnServiceStopped(ServiceStopped message)
		{
			if (_stopping)
			{
				_controllerChannel.Send(new UnloadService
					{
						ServiceName = message.ServiceName
					});
			}

			_updated.Set();
		}

		void StopServices(TimeSpan timeout)
		{
			_stopping = true;

			SendStopCommandToServices();

			DateTime stopTime = SystemUtil.Now + timeout;

			while (SystemUtil.Now < stopTime)
			{
				_updated.WaitOne(1.Seconds());

				if (AllServicesAreCompleted())
					break;
			}
		}

		bool AllServicesAreCompleted()
		{
			return !_serviceCache.Any(x => x.CurrentState != ShelfService.Completed);
		}

		void SendStopCommandToServices()
		{
			_serviceCache.Each((name, service) =>
				{
					var message = new StopService
						{
							ServiceName = name
						};

					_controllerChannel.Send(message);
				});
		}
	}
}