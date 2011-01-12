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
namespace Topshelf.Model
{
	using Messages;
	using Stact.Workflow;


	public interface IServiceWorkflow
	{
		Event<CreateService> Create { get; }
		Event<StartService> Start { get; }
		Event<StopService> Stop { get; }
		Event<UnloadService> Unload { get; }
		Event<PauseService> Pause { get; }
		Event<ContinueService> Continue { get; }
		Event<RestartService> Restart { get; }

	
		Event<ServiceCreated> OnCreated { get; }
		Event<ServiceRunning> OnRunning { get; }
		Event<ServiceStopped> OnStopped { get; }
		Event<ServicePaused> OnPaused { get; }
		Event<ServiceUnloaded> OnUnloaded { get; }
		Event<ServiceFault> OnFaulted { get; }


		State Initial { get; }
		State Creating { get; }
		State Created { get; }
		State Starting { get; }
		State Running { get; }
		State Pausing { get; }
		State Paused { get; }
		State Continuing { get; }
		State StopRequested { get; }
		State Stopping { get; }
		State Stopped { get; }
		State StoppingToRestart { get; }
		State CreatingToRestart { get; }
		State Restarting { get; }
		State Unloading { get; }
		State Completed { get; }
		State Faulted { get; }
	}
}