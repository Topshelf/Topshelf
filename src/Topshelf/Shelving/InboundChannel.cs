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
	using Magnum.Channels;
	using Magnum.Channels.Configuration;


	public class InboundChannel :
		UntypedChannel,
		IDisposable
	{
		UntypedChannel _channel;
		ChannelConnection _connection;
		bool _disposed;

		public InboundChannel(Uri address, string endpoint, Action<ConnectionConfigurator> configurator)
		{
			_channel = new ChannelAdapter();
			_connection = _channel.Connect(x =>
				{
					x.ReceiveFromWcfChannel(address, endpoint)
						.ExecuteOnThreadPoolFiber();

					configurator(x);
				});
		}

		public InboundChannel(Uri address, string endpoint)
			: this(address, endpoint, x => { })
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~InboundChannel()
		{
			Dispose(false);
		}

		public void Send<T>(T message)
		{
			_channel.Send(message);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_connection != null)
				{
					_connection.Dispose();
					_connection = null;
				}

				_channel = null;
			}

			_disposed = true;
		}
	}
}