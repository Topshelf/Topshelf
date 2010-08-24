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
	using log4net;
	using Magnum.Channels;


	public class OutboundChannel :
		UntypedChannel,
		IDisposable
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(OutboundChannel));

		ChannelAdapter _channel;
		ChannelConnection _connection;

		bool _disposed;

		public OutboundChannel(Uri address, string endpoint)
		{
			_log.DebugFormat("Opening outbound channel at {0} ({1})", address, endpoint);

			_channel = new ChannelAdapter();
			_connection = _channel.Connect(cc =>
				{
					cc.SendToWcfChannel(address, endpoint)
						.HandleOnFiber();
				});
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Send<T>(T message)
		{
			if (!_disposed)
				_channel.Send(message);
		}

		~OutboundChannel()
		{
			Dispose(false);
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