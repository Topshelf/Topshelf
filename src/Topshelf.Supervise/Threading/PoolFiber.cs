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
namespace Topshelf.Supervise.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// An Fiber that uses the .NET ThreadPool and QueueUserWorkItem to execute
    /// actions.
    /// </summary>
    class PoolFiber :
        Fiber
    {
        readonly OperationExecutor _executor;
        readonly object _lock = new object();
        IList<Action> _empty = new List<Action>();

        bool _executorQueued;
        IList<Action> _operations = new List<Action>();
        bool _shuttingDown;

        public PoolFiber()
            : this(new TryCatchOperationExecutor())
        {
        }

        public PoolFiber(OperationExecutor executor)
        {
            _executor = executor;
        }

        public void Add(Action operation)
        {
            if (_shuttingDown)
                return;

            lock (_lock)
            {
                _operations.Add(operation);
                if (!_executorQueued)
                    QueueWorkItem();
            }
        }

        public void Shutdown(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero)
            {
                lock (_lock)
                {
                    _shuttingDown = true;
                }

                return;
            }

            DateTime waitUntil = DateTime.Now + timeout;

            lock (_lock)
            {
                _shuttingDown = true;
                Monitor.PulseAll(_lock);

                while (_operations.Count > 0 || _executorQueued)
                {
                    timeout = waitUntil - DateTime.Now;
                    if (timeout < TimeSpan.Zero)
                        throw new TopshelfException(
                            "Timeout expired waiting for all pending actions to complete during shutdown");

                    Monitor.Wait(_lock, timeout);
                }
            }
        }

        public void Stop()
        {
            _shuttingDown = true;

            _executor.Stop();
        }

        public override string ToString()
        {
            return string.Format("{0} (Count: {1})", typeof(PoolFiber).Name, _operations.Count);
        }

        void QueueWorkItem()
        {
            if (!ThreadPool.QueueUserWorkItem(x => Execute()))
                throw new TopshelfException("QueueUserWorkItem did not accept our execute method");

            _executorQueued = true;
        }

        bool Execute()
        {
            IList<Action> operations = RemoveAll();

            _executor.Execute(operations, remaining =>
                {
                    lock (_lock)
                    {
                        int i = 0;
                        foreach (Action action in remaining)
                            _operations.Insert(i++, action);
                    }
                });

            lock (_lock)
            {
                if (_operations.Count == 0)
                {
                    _executorQueued = false;

                    Monitor.PulseAll(_lock);
                }
                else
                    QueueWorkItem();
            }

            return true;
        }

        IList<Action> RemoveAll()
        {
            lock (_lock)
            {
                if (_operations.Count == 0)
                    return _empty;

                IList<Action> operations = _operations;

                _operations = new List<Action>();

                return operations;
            }
        }
    }
}