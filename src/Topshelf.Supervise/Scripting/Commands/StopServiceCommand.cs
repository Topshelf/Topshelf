// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Supervise.Scripting.Commands
{
    using System;
    using Runtime;

    public class StopServiceCommand :
        Command
    {
        readonly Guid _compensateId;
        readonly Guid _executeId;

        public StopServiceCommand()
        {
            _executeId = new Guid("A7E5B54C-D695-4C9C-96CB-AB0D68F2A0F6");
            _compensateId = new Guid("7AA08166-F419-4B69-8672-DA116813E148");
        }

        public CommandScriptStepAudit Execute(CommandScriptStep task)
        {
            var hostControl = task.CommandScript.Variables.Get<HostControl>();

            bool stopped = false;

            var serviceHandle = task.Arguments.Get<ServiceHandle>();
            if(serviceHandle != null)
            {
                stopped = serviceHandle.Stop(hostControl);
            }
            return new CommandScriptStepAudit(this, new CommandScriptStepResult
                {
                    serviceHandle,
                    {"stopped", stopped}
                });
        }

        public bool Compensate(CommandScriptStepAudit audit, CommandScript commandScript)
        {
            // we can't unstop a service
            return false;
        }

        public Guid ExecuteId
        {
            get { return _executeId; }
        }

        public Guid CompensateId
        {
            get { return _compensateId; }
        }
    }
}