// Copyright 2007-2008 The Apache Software Foundation.
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

    public class ErrorMessage
    {
        public ErrorMessage(Exception ex)
        {
            Ex = ex;
        }

        public Exception Ex { get; private set; }
    }

    public class ServiceMessage
    {
        public string ServiceId { get; set; }
        public Guid ConversationId { get; set; }
    }

    //host commands
    public class RunAsConsole
    {
    }

    public class Install
    {
    }

    // instance name
    public class Uninstall
    {
    }

    //instance name
    public class RunAsService
    {
    }

    //instance name


    //commands
    public class StartService :
        ServiceMessage
    {
    }

    public class StopService :
        ServiceMessage
    {
    }

    public class ContinueService :
        ServiceMessage
    {
    }

    public class PauseService :
        ServiceMessage
    {
    }

    //events
    public class ServiceStarted :
        ServiceMessage
    {
    }

    public class ServiceStopped :
        ServiceMessage
    {
    }

    public class ServicePaused :
        ServiceMessage
    {
    }

    public class ServiceContinued :
        ServiceMessage
    {
    }

    public class ReadyService :
        ServiceMessage
    {
    }

    public class FileSystemChange :
        ServiceMessage
    {
    }
}