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
namespace Topshelf.Supervise.Scripting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Logging;

    public class CommandScript :
        IEnumerable<CommandScriptStep>
    {
        readonly Stack<CommandScriptStepAudit> _completedTasks;
        readonly IList<Exception> _exceptions;
        readonly LogWriter _log = HostLogger.Get<CommandScript>();
        readonly Queue<CommandScriptStep> _nextTask;
        readonly CommandScriptDictionary _variables;

        public CommandScript(params CommandScriptStep[] steps)
        {
            _nextTask = new Queue<CommandScriptStep>();
            _completedTasks = new Stack<CommandScriptStepAudit>();
            _exceptions = new List<Exception>();
            _variables = new CommandScriptDictionary();

            foreach (CommandScriptStep step in steps)
                Add(step);
        }

        public CommandScriptDictionary Variables
        {
            get { return _variables; }
        }

        public void Add(CommandScriptStep step)
        {
            step.CommandScript = this;
            _nextTask.Enqueue(step);
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

            CommandScriptStep currentItem = _nextTask.Dequeue();
            var command = (Command)Activator.CreateInstance(currentItem.ActivityType);
            try
            {
                CommandScriptStepAudit audit = command.Execute(currentItem);
                if (audit != null)
                {
                    _completedTasks.Push(audit);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error("CommandScript Exception", ex);
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

            CommandScriptStepAudit currentItem = _completedTasks.Pop();
            var activity = (Command)Activator.CreateInstance(currentItem.CommandType);
            try
            {
                return activity.Compensate(currentItem, this);
            }
            catch (Exception ex)
            {
                _log.Error("CommandScript Compensation Exception", ex);
                throw;
            }
        }

        public IEnumerator<CommandScriptStep> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}