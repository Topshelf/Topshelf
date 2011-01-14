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
namespace Stuff
{
	using System;
	using System.Configuration;
	using System.Threading;


	public class TownCrier
	{
		FailureType _failureMode;
		readonly System.Timers.Timer _timer;

		public TownCrier()
		{
			string value = ConfigurationManager.AppSettings["FailureType"] ?? "none";

			_failureMode = (FailureType)Enum.Parse(typeof(FailureType), value, true);

			if (_failureMode == FailureType.FailToCreate)
				throw new InvalidOperationException("TownCrier was configured to fail on create");

			_timer = new System.Timers.Timer(1000)
				{
					AutoReset = true
				};

			int loopCount = 0;
			_timer.Elapsed += (sender, eventArgs) =>
				{
					Console.WriteLine(DateTime.Now);
					loopCount++;

					if (_failureMode == FailureType.CrashAfterStart && loopCount > 10)
					{
						_failureMode = FailureType.None;

						ThreadPool.QueueUserWorkItem(x =>
							{
								throw new InvalidOperationException("TownCrier was configured to fail after startup");
							});
					}
				};
		}

		public void Start()
		{
			if (_failureMode == FailureType.FailToStart)
				throw new InvalidOperationException("TownCrier was configured to fail on start");

			_timer.Start();
		}

		public void Stop()
		{
			if (_failureMode == FailureType.FailToStop)
				throw new InvalidOperationException("TownCrier was configured to fail on stop");

			_timer.Stop();
		}
	}
}