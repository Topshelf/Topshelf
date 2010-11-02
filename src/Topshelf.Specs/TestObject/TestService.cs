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
namespace Topshelf.Specs.TestObject
{
	using System;
	using Magnum;


	public class TestService
	{
		readonly Action _startAction;
		public Future<bool> WasContinued { get; private set; }
		public Future<bool> WasRunning { get; private set; }
		public Future<bool> Running { get; private set; }
		public Future<bool> Paused { get; private set; }
		public Future<bool> Stopped { get; private set; }

		public TestService(Action startAction)
			: this()
		{
			_startAction = startAction;
		}

		public TestService()
		{
			Running = new Future<bool>();
			Stopped = new Future<bool>();
			Paused = new Future<bool>();
			WasContinued = new Future<bool>();
			WasRunning = new Future<bool>();
		}

		public void Start()
		{
			if (Running.IsCompleted)
				throw new InvalidOperationException("Service was running and is being started again");

			if (_startAction != null)
				_startAction();

			WasRunning.Complete(true);
			Running.Complete(true);
		}

		public void Stop()
		{
			if (!Running.IsCompleted)
				throw new InvalidOperationException("Service was not running but is being stopped");

			Stopped.Complete(true);
		}

		public void Pause()
		{
			if (!Running.IsCompleted)
				throw new InvalidOperationException("Service was not running but is being paused");

			Paused.Complete(true);
		}

		public void Continue()
		{
			if (!Paused.IsCompleted)
				throw new InvalidOperationException("Service was not paused but is being continued");

			WasContinued.Complete(true);
		}
	}
}