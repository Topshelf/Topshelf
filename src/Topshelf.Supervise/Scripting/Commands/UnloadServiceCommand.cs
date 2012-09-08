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

    public class UnloadServiceCommand :
        Command
    {
        readonly Guid _compensateId;
        readonly Guid _executeId;

        public UnloadServiceCommand()
        {
            _executeId = new Guid("C9181494-9817-4751-9A46-D5D10D9E972D");
            _compensateId = new Guid("A30EE035-9609-48E6-988A-17D58344C023");
        }

        public CommandScriptStepAudit Execute(CommandScriptStep task)
        {
            bool unloaded = false;
            var serviceHandle = task.Arguments.Get<ServiceHandle>();

            if (serviceHandle != null)
            {
                serviceHandle.Dispose();

                unloaded = true;

                task.Arguments.Set<ServiceHandle>(null);
            }

            return new CommandScriptStepAudit(this, new CommandScriptStepResult {{"unloaded", unloaded}});
        }

        public bool Compensate(CommandScriptStepAudit audit, CommandScript commandScript)
        {
            // we can't really reload an AppDomain now, can we?

            return true;
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