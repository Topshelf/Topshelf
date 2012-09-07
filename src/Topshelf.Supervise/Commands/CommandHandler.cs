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
        bool Handle(Guid commandId, CommandScript commandScript);
    }


    /// <summary>
    ///   The generic command handler will build a command and determine if it can handle the command. If it can, rock on, otherwise, it skips
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class CommandHandler<T> :
        CommandHandler
        where T : Command, new()
    {
        readonly CommandHandler _nextHandler;

        public CommandHandler(CommandHandler commandHandler)
        {
            _nextHandler = commandHandler;
        }

        public bool Handle(Guid commandId, CommandScript commandScript)
        {
            var command = new T();

            if (command.CompensateId.Equals(commandId))
                return HandleCompensation(commandScript);

            if (command.ExecuteId.Equals(commandId))
                return HandleCommand(commandScript);

            return false;
        }

        protected bool HandleCommand(CommandScript commandScript)
        {
            if (commandScript.IsCompleted)
                return false;

            if (commandScript.ExecuteNext())
                return _nextHandler.Handle(commandScript.NextCommandId, commandScript);

            return _nextHandler.Handle(commandScript.CompensationId, commandScript);
        }

        protected bool HandleCompensation(CommandScript commandScript)
        {
            if (!commandScript.IsInProgress)
                return false;

            if (commandScript.UndoLast())
                return _nextHandler.Handle(commandScript.CompensationId, commandScript);

            return _nextHandler.Handle(commandScript.NextCommandId, commandScript);
        }
    }
}