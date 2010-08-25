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
	using log4net;
	using Magnum.Channels;
	using Magnum.Channels.Configuration;
	using Shelving;


	public class InboundChannel : 
		ServiceChannelBase
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(InboundChannel));

		public InboundChannel(Uri address, string pipeName, Action<ConnectionConfigurator> configurator)
			: base(address, pipeName, x =>
				{
					configurator(x);

					x.ReceiveFromWcfChannel(address, pipeName)
						.HandleOnFiber();
				})
		{
			_log.DebugFormat("Opening inbound channel at {0} ({1})", address, pipeName);
		}

		public InboundChannel(Uri address, string pipeName)
			: this(address, pipeName, x => { })
		{
		}
	}
}