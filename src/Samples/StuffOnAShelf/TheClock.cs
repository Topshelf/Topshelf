// Copyright 2007-2010 The Apache Software Foundation.
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
namespace StuffOnAShelf
{
	using System;
	using System.Configuration;
	using System.Threading;
	using log4net;


	public class TheClock
	{
		readonly ILog _log = LogManager.GetLogger(typeof(TheClock));
		Timer _timer;
		int _count;
		ClockFailureMode _failureMode;

		public TheClock()
		{
			string value = ConfigurationManager.AppSettings["badTidings"] ?? "none";

			_failureMode = (ClockFailureMode)Enum.Parse(typeof(ClockFailureMode), value, true);

			if (_failureMode == ClockFailureMode.FailToCreate)
				throw new InvalidOperationException("Clock was configured to fail on create");
		}

		void ClockInterval(object value)
		{
			_log.Info(DateTime.Now);

			_count++;
			if (_failureMode == ClockFailureMode.CrashAfterStart && _count >= 10)
				throw new InvalidOperationException("Clock was configured to die after start");

			_timer.Change(1000, Timeout.Infinite);
		}

		public void Start()
		{
			if (_failureMode == ClockFailureMode.FailToStart)
				throw new InvalidOperationException("Clock was configured to fail on startup");

			_timer = new Timer(ClockInterval, null, 1000, Timeout.Infinite);
		}

		public void Stop()
		{
			if (_failureMode == ClockFailureMode.FailToStop)
				throw new InvalidOperationException("Clock was configured to fail on stop");

			_timer.Dispose();
			_timer = null;
		}
	}
}