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
	using System;
	using Stact;
	using Stact.Workflow;


	public interface IServiceController :
		Actor,
		IDisposable
	{
		Type ServiceType { get; }
		State CurrentState { get; set; }

		string Name { get; }

		void Create();

		void Start();

		void Stop();

		void Unload();

		void Pause();
		void Continue();


//		void Created(ServiceCreated message);
//		void Running();
//		void Stopped();
//		void Restarted();
//		void Paused();
//		void Unloaded();
//		void Completed();
//		void Faulted(ServiceFault message);
//		void HandleException<TBody>(TBody message);
	}


	public interface IServiceController<TService> :
		IServiceController
		where TService : class
	{
	}
}