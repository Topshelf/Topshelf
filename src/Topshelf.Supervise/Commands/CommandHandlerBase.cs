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
    public abstract class CommandHandlerBase
    {
        readonly CommandHandler _nextHandler;

        protected CommandHandlerBase(CommandHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        protected bool HandleCommand(WorkList workList)
        {
            if (workList.IsCompleted)
                return false;

            if (workList.ExecuteNext())
                return _nextHandler.Handle(workList.NextCommandId, workList);

            return _nextHandler.Handle(workList.CompensationId, workList);
        }

        protected bool HandleCompensation(WorkList workList)
        {
            if (!workList.IsInProgress)
                return false;

            if (workList.UndoLast())
                return _nextHandler.Handle(workList.CompensationId, workList);

            return _nextHandler.Handle(workList.NextCommandId, workList);
        }
    }
}