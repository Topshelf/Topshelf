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
namespace Topshelf.Supervise
{
    using System;
    using System.Linq;
    using Commands;

    public class DispatchCommandHandler :
        CommandHandler
    {
        readonly CommandHandler[] _handlers;
        readonly WorkList _workList;

        public DispatchCommandHandler(WorkList workList, params Func<CommandHandler, CommandHandler>[] handlers)
        {
            _workList = workList;
            _handlers = handlers.Select(x => x(this)).ToArray();
        }

        public bool Handle(Guid commandId, WorkList workList)
        {
            return _handlers.Any(handler => handler.Handle(commandId, workList));
        }

        public bool Execute()
        {
            return Handle(_workList.NextCommandId, _workList);
        }
    }
}