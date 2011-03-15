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
namespace Topshelf.Messages
{
	using System;
	using System.Text;
	using Internal;
	using Magnum.Extensions;


	public static class ServiceFaultExtentions
	{
		public static string ToLogString([NotNull] this ServiceFault message)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			var sb = new StringBuilder();

			if (message.ExceptionDetail != null)
			{
				sb.AppendLine(message.ExceptionDetail.Message);
				sb.AppendLine(message.ExceptionDetail.StackTrace);
			}

			if (message.InnerExceptions != null)
			{
				message.InnerExceptions.Each(ed =>
					{
						sb.AppendLine(ed.Message);
						sb.AppendLine(ed.StackTrace);
					});
			}

			return sb.ToString();
		}
	}
}