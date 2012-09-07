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

    /// <summary>
    ///   A command handler executes a command
    /// </summary>
    public interface CommandHandler
    {
        bool Handle(Guid commandId, WorkList workList);
    }


    /// <summary>
    /// The generic command handler will build a command and determine
    /// if it can handle the command. If it can, rock on, otherwise, it skips
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandHandler<T> :
        CommandHandlerBase,
        CommandHandler
        where T : Command, new()
    {
        public CommandHandler(CommandHandler commandHandler)
            : base(commandHandler)
        {
        }

        public bool Handle(Guid commandId, WorkList workList)
        {
            var command = new T();

            if (command.CompensateId.Equals(commandId))
                return HandleCompensation(workList);
            
            if (command.ExecuteId.Equals(commandId))
                return HandleCommand(workList);

            return false;
        }
    }
}