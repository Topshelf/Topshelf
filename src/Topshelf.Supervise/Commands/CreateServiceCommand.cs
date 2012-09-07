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
    using HostConfigurators;
    using Runtime;

    public class CreateServiceCommand :
        Command
    {
        readonly Guid _compensateId;
        readonly Guid _executeId;

        public CreateServiceCommand()
        {
            _executeId = new Guid("A141E945-0A05-4833-B496-EA0630AC052A");
            _compensateId = new Guid("03419435-7128-4ECE-9643-BFBC78A0175D");
        }

        public CommandScriptStepAudit Execute(CommandScriptStep task)
        {
            ServiceHandle serviceHandle;
            task.CommandScript.Variables.TryGetValue(out serviceHandle);

            var result = new CommandScriptStepResult();

            if (serviceHandle == null)
            {
                var settings = task.Arguments.Get<HostSettings>();
                var hostControl = task.Arguments.Get<HostControl>();
                var serviceBuilderFactory = task.Arguments.Get<ServiceBuilderFactory>();

                serviceHandle = new ServiceHandleProxy(settings, hostControl, serviceBuilderFactory);

                result.Add(serviceHandle);
            }

            return new CommandScriptStepAudit(this, result);
        }

        public bool Compensate(CommandScriptStepAudit audit, CommandScript commandScript)
        {
            ServiceHandle serviceHandle;
            if (audit.Result.TryGetValue(out serviceHandle))
            {
                serviceHandle.Dispose();
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