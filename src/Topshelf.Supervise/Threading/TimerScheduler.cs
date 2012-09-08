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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;

    [DebuggerDisplay("{GetType().Name} ( Count: {Count}, Next: {NextActionTime} )")]
    public class TimerScheduler :
        Scheduler
    {
        readonly Fiber _fiber;
        readonly object _lock = new object();
        readonly TimeSpan _noPeriod = TimeSpan.FromMilliseconds(-1);
        readonly ScheduledOperationList _operations = new ScheduledOperationList();
        bool _stopped;
        Timer _timer;

        public TimerScheduler(Fiber fiber)
        {
            _fiber = fiber;
        }

        static DateTime Now
        {
            get { return DateTime.UtcNow; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected int Count
        {
            get { return _operations.Count; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected string NextActionTime
        {
            get
            {
                DateTime scheduledAt;
                if (_operations.GetNextScheduledTime(Now, out scheduledAt))
                    return scheduledAt.ToString();

                return "None";
            }
        }

        public ScheduledOperation Schedule(TimeSpan interval, Fiber fiber, Action operation)
        {
            var scheduled = new ScheduledOperationExecuterImpl(GetScheduledTime(interval), fiber, operation);
            Schedule(scheduled);

            return scheduled;
        }

        public ScheduledOperation Schedule(TimeSpan interval, TimeSpan periodicInterval, Fiber fiber, Action operation)
        {
            ScheduledOperationExecuterImpl scheduled = null;
            scheduled = new ScheduledOperationExecuterImpl(GetScheduledTime(interval), fiber, () =>
                {
                    try
                    {
                        operation();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        scheduled.ScheduledAt = GetScheduledTime(periodicInterval);
                        Schedule(scheduled);
                    }
                });
            Schedule(scheduled);

            return scheduled;
        }

        public void Stop(TimeSpan timeout)
        {
            _stopped = true;

            lock (_lock)
            {
                if (_timer != null)
                    _timer.Dispose();

                _fiber.Shutdown(timeout);
            }
        }

        void Schedule(ScheduledOperationExecuter action)
        {
            _fiber.Add(() =>
                {
                    _operations.Add(action);

                    ExecuteExpiredActions();
                });
        }

        void ScheduleTimer()
        {
            DateTime now = Now;

            DateTime scheduledAt;
            if (_operations.GetNextScheduledTime(now, out scheduledAt))
            {
                lock (_lock)
                {
                    TimeSpan dueTime = scheduledAt - now;

                    if (_timer != null)
                        _timer.Change(dueTime, _noPeriod);
                    else
                        _timer = new Timer(x => _fiber.Add(ExecuteExpiredActions), this, dueTime, _noPeriod);
                }
            }
        }

        void ExecuteExpiredActions()
        {
            if (_stopped)
                return;

            ScheduledOperationExecuter[] expiredActions;
            while ((expiredActions = _operations.GetExpiredActions(Now)).Length > 0)
            {
                foreach (ScheduledOperationExecuter action in expiredActions)
                {
                    try
                    {
                        action.Execute();
                    }
                    catch
                    {
                    }
                }
            }

            ScheduleTimer();
        }

        static DateTime GetScheduledTime(TimeSpan interval)
        {
            return Now + interval;
        }
    }
}