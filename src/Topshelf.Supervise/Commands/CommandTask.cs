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

    public interface CommandTask
    {
        WorkList WorkList { get; }
        CommandTaskArguments Arguments { get; }
        Type ActivityType { get; }
    }

    public class CommandTask<T> :
        CommandTask
        where T : Command
    {
        public CommandTask(CommandTaskArguments arguments)
        {
            Arguments = arguments;
        }

        public WorkList WorkList { get; set; }

        public CommandTaskArguments Arguments { get; private set; }

        public Type ActivityType
        {
            get { return typeof(T); }
        }
    }
}