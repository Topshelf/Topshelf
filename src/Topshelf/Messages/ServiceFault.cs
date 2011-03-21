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
	using System.Collections.Generic;


	public class ServiceFault :
		ServiceEvent
	{
		protected ServiceFault()
		{
		}

		public ServiceFault(string serviceName, Exception ex)
			: base(serviceName)
		{
			InnerExceptions = new List<ExceptionDetail>();

			EventType = ServiceEventType.Fault;

			if (ex == null)
				return;
			
			ExceptionDetail = DetailsFor(ex);
			RecordInnerException(ex.InnerException);
		}

		public IList<ExceptionDetail> InnerExceptions { get; private set; }

		public ExceptionDetail ExceptionDetail { get; private set; }

		static ExceptionDetail DetailsFor(Exception ex)
		{
			return new ExceptionDetail(ex.GetType().FullName,
			                           ex.Message,
			                           ex.StackTrace,
			                           ex.HelpLink,
			                           ex.ToString());
		}

		void RecordInnerException(Exception ex)
		{
			if (ex == null)
				return;

			InnerExceptions.Add(DetailsFor(ex));
			RecordInnerException(ex.InnerException);
		}
	}
}