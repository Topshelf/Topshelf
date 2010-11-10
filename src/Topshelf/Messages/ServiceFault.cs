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
        public ServiceFault(string serviceName, Exception ex)
            : base(serviceName)
        {
            InnerExceptions = new List<ExceptionDetail>();

            EventType = ServiceEventType.Fault;

            if (ex != null)
            {
                ExceptionDetail = new ExceptionDetail
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace
                    };
                
                RecordInnerException(ex.InnerException);
            }
        }

        protected ServiceFault()
        {
        }

        public IList<ExceptionDetail> InnerExceptions { get; protected set; }

        public ExceptionDetail ExceptionDetail { get; protected set; }

        void RecordInnerException(Exception ex)
        {
            if (ex == null)
                return;

            InnerExceptions.Add(new ExceptionDetail
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                });

            RecordInnerException(ex.InnerException);
        }
    }
}