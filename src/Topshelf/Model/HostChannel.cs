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
	using log4net;
	using Magnum.Extensions;
	using Stact;
	using Stact.Configuration;


	public class HostChannel :
		UntypedChannel,
		IServiceChannel,
		IDisposable
	{
		readonly Uri _address;
		readonly ILog _log = LogManager.GetLogger("Topshelf.Model.HostChannel");
		readonly UntypedChannel _output;
		readonly string _pipeName;
		IList<ChannelConnection> _connections;
		bool _disposed;
		WcfChannelHost _host;

		public HostChannel(Uri address, string pipeName, Action<ConnectionConfigurator> configurator)
			: this(new ChannelAdapter(), address, pipeName)
		{
			_connections = new List<ChannelConnection>();

			_connections.Add(_output.Connect(configurator));
		}

		public HostChannel(UntypedChannel output, Uri address, string pipeName)
		{
			_output = output;
			_address = address;
			_pipeName = pipeName;

			_host = new WcfChannelHost(new SynchronousFiber(), output, address, pipeName);
		}

		public Uri Address
		{
			get { return _address; }
		}

		public string PipeName
		{
			get { return _pipeName; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Connect(Action<ConnectionConfigurator> configurator)
		{
			_connections.Add(_output.Connect(configurator));
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_host != null)
				{
					_host.Stop();
					_host.Dispose();
					_host = null;
				}

				if (_connections != null)
				{
					_connections.Each(x =>
						{
							try
							{
								x.Dispose();
							}
							catch (Exception ex)
							{
								_log.Error("Closing connection caused an exception", ex);
							}
						});
					_connections = null;
				}
			}

			_disposed = true;
		}

		public void Send<T>(T message)
		{
			_output.Send(message);
		}
	}
}