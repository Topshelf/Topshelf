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

        public CommandAudit Execute(CommandTask task)
        {
            var serviceHandle = task.Arguments.Get<ServiceHandle>();
            var hostControl = task.Arguments.Get<HostControl>();

            bool stopped = serviceHandle.Stop(hostControl);

            return new CommandAudit(this, new CommandResult
                {
                    {"stopped", stopped}
                });
        }

        public bool Compensate(CommandAudit audit, WorkList workList)
        {
            var stopped = audit.Get<bool>("stopped");

            // should probably restart the service here, don't you think?

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