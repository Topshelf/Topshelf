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
	using System.IO;
	using System.Linq;
	using Magnum.Extensions;
	using Stact;
	using Stact.ServerFramework;


	public class CssConnectionHandler :
		PatternMatchConnectionHandler
	{
		readonly CssChannel _statusChannel;

		public CssConnectionHandler()
			:
				base(".css$", "GET")
		{
			_statusChannel = new CssChannel();
		}

		protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
		{
			return _statusChannel;
		}


		class CssChannel :
			Channel<ConnectionContext>
		{
			readonly Fiber _fiber;

			public CssChannel()
			{
				_fiber = new PoolFiber();
			}

			public void Send(ConnectionContext context)
			{
				_fiber.Add(() =>
					{
						string localPath = context.Request.Url.LocalPath;
						string cssName = localPath.Split('/').Last();
						context.Response.ContentType = "text/css";
						using (Stream str = GetType().Assembly.GetManifestResourceStream("Topshelf.Dashboard.styles." + cssName))
						{
							byte[] buff = str.ReadToEnd();
							context.Response.OutputStream.Write(buff, 0, buff.Length);
						}
						context.Complete();
					});
			}
		}
	}
}