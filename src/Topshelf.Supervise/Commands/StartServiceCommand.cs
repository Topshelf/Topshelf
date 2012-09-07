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
namespace Topshelf.Supervise.Commands
{
    using System;
    using Runtime;

    public class StartServiceCommand :
        Command
    {
        readonly Guid _compensateId;
        readonly Guid _executeId;

        public StartServiceCommand()
        {
            _executeId = new Guid("2E5B29D5-9C22-4C18-BAA5-FC3E36EBF4D9");
            _compensateId = new Guid("9E9B8C61-2072-4910-9A52-410D887D0286");
        }

        public CommandScriptStepAudit Execute(CommandScriptStep task)
        {
            var serviceHandle = task.CommandScript.Variables.Get<ServiceHandle>();

            if (serviceHandle == null)
                throw new ServiceControlException("The service handle was null and could not be started.");

            var hostControl = task.Arguments.Get<HostControl>();

            bool started = serviceHandle.Start(hostControl);

            return new CommandScriptStepAudit(this, new CommandScriptStepResult {started});
        }

        public bool Compensate(CommandScriptStepAudit audit, CommandScript commandScript)
        {
            var started = audit.Result.Get<bool>("started");

            if (started)
            {
                var serviceHandle = audit.Result.Get<ServiceHandle>();
                var hostControl = audit.Result.Get<HostControl>();

                return serviceHandle.Stop(hostControl);
            }

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