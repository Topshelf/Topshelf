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
namespace Topshelf.Model
{
	using System;
	using System.Collections.Generic;
	using Magnum.Extensions;
	using Stact;
	using Stact.Configuration;


	public class ServiceChannel :
		IServiceChannel,
		UntypedChannel,
		IDisposable
	{
		UntypedChannel _channel;
		IList<ChannelConnection> _connections;
		bool _disposed;

		protected ServiceChannel(Action<ConnectionConfigurator> configurator)
		{
			_channel = new ChannelAdapter();
			_connections = new List<ChannelConnection>();
			_connections.Add(_channel.Connect(configurator));
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Send<T>(T message)
		{
			_channel.Send(message);
		}

		public void Connect(Action<ConnectionConfigurator> configurator)
		{
			_connections.Add(_channel.Connect(configurator));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				_connections.Each(x => x.Dispose());
				_connections = null;

				_channel = null;
			}

			_disposed = true;
		}
	}
}