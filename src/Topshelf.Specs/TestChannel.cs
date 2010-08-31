namespace Topshelf.Specs
{
	using System;
	using System.Collections.Generic;
	using Magnum.Channels;
	using Magnum.Channels.Configuration;
	using Magnum.Extensions;
	using Model;


	public class TestChannel :
		ServiceChannel,
		IDisposable
	{
		UntypedChannel _channel = new ChannelAdapter();
		IList<ChannelConnection> _connections = new List<ChannelConnection>();

		public void Dispose()
		{
			_connections.Each(x => x.Dispose());
		}

		public void Send<T>(T message)
		{
			_channel.Send(message);
		}

		public string PipeName
		{
			get { throw new NotImplementedException(); }
		}

		public Uri Address
		{
			get { throw new NotImplementedException(); }
		}

		public void Connect(Action<ConnectionConfigurator> configurator)
		{
			_connections.Add(_channel.Connect(configurator));
		}
	}
}