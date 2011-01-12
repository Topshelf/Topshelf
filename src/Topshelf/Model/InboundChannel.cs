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
	using Stact;
	using Stact.Configuration;


	public class InboundChannel :
		ServiceChannel
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Model.InboundChannel");
		WcfChannelHost _host;

		public InboundChannel(Uri address, string pipeName, Action<ConnectionConfigurator> configurator)
			: base(configurator)
		{
			_log.DebugFormat("Opening inbound channel at {0} ({1})", address, pipeName);

			_host = new WcfChannelHost(new SynchronousFiber(), this, address, pipeName);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_host != null)
				{
					_host.Dispose();
					_host = null;
				}
			}

			base.Dispose(disposing);
		}
	}
}