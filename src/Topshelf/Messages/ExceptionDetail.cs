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

	[Serializable]
	public class ExceptionDetail
	{
		public ExceptionDetail(string exceptionTypeName, string message, string stackTrace, string helpLink, string stringRepresentation)
		{
			ExceptionTypeName = exceptionTypeName;
			Message = message;
			StackTrace = stackTrace;
			HelpLink = helpLink;
			StringRepresentation = stringRepresentation;
		}

		public string ExceptionTypeName { get; private set; }
		public string Message { get; private set; }
		public string StackTrace { get; private set; }
		public string HelpLink { get; private set; }
		public string StringRepresentation { get; private set; }
	}
}