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
	using System;
	using System.IO;
	using System.Linq;
	using Magnum.Extensions;
	using Stact;
	using Stact.ServerFramework;


	public class StaticResourceHandler :
		PatternMatchConnectionHandler
	{
		readonly StaticResourceChannel _channel;
		public StaticResourceHandler(String pattern, String resourcePrefix, String contentType, params String [] supportedVerbs) :
			base(pattern, supportedVerbs)
		{
			_channel = new StaticResourceChannel(resourcePrefix, contentType);
		}

		protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
		{
			return _channel;
		}

		class StaticResourceChannel : 
			Channel<ConnectionContext>
		{
			readonly Fiber _fiber;
			readonly string _resourcePrefix;
			readonly string _contentType;

			public StaticResourceChannel(String resourcePrefix, String contentType)
			{
				_fiber = new PoolFiber();
				_resourcePrefix = resourcePrefix;

				_contentType = contentType;// "text/css";
			}
			public void Send(ConnectionContext context)
			{
				_fiber.Add(() =>
				{
					string localPath = context.Request.Url.LocalPath;
					string cssName = localPath.Split('/').Last();
					context.Response.ContentType = _contentType;
					//_resourcePrefix = "Topshelf.Dashboard.styles.";
					using (Stream str = GetType().Assembly.GetManifestResourceStream(_resourcePrefix + cssName))
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