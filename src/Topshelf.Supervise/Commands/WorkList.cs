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
    using System.Collections.Generic;
    using Logging;

    public class WorkList
    {
        readonly Stack<CommandAudit> _completedTasks;
        readonly IList<Exception> _exceptions;
        readonly LogWriter _log = HostLogger.Get<WorkList>();
        readonly Queue<CommandTask> _nextTask;

        public WorkList(params CommandTask[] tasks)
        {
            _nextTask = new Queue<CommandTask>();
            _completedTasks = new Stack<CommandAudit>();
            _exceptions = new List<Exception>();

            foreach (CommandTask task in tasks)
                _nextTask.Enqueue(task);
        }

        public bool IsCompleted
        {
            get { return _nextTask.Count == 0; }
        }

        public bool IsInProgress
        {
            get { return _completedTasks.Count > 0; }
        }

        public Guid NextCommandId
        {
            get
            {
                if (IsCompleted)
                    return Guid.Empty;

                return ((Command)Activator.CreateInstance(_nextTask.Peek().ActivityType)).ExecuteId;
            }
        }

        public Guid CompensationId
        {
            get
            {
                if (!IsInProgress)
                    return Guid.Empty;

                return ((Command)Activator.CreateInstance(_completedTasks.Peek().CommandType)).CompensateId;
            }
        }

        public bool ExecuteNext()
        {
            if (IsCompleted)
            {
                throw new InvalidOperationException();
            }

            CommandTask currentItem = _nextTask.Dequeue();
            var command = (Command)Activator.CreateInstance(currentItem.ActivityType);
            try
            {
                CommandAudit audit = command.Execute(currentItem);
                if (audit != null)
                {
                    _completedTasks.Push(audit);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error("WorkList Exception", ex);
                _exceptions.Add(ex);
            }
            return false;
        }

        public bool UndoLast()
        {
            if (!IsInProgress)
            {
                throw new InvalidOperationException();
            }

            CommandAudit currentItem = _completedTasks.Pop();
            var activity = (Command)Activator.CreateInstance(currentItem.CommandType);
            try
            {
                return activity.Compensate(currentItem, this);
            }
            catch (Exception ex)
            {
                _log.Error("WorkList Compensation Exception", ex);
                throw;
            }
        }
    }
}