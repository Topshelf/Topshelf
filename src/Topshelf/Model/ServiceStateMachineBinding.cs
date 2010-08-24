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
	using Magnum.StateMachine;


	public class ServiceStateMachineBinding :
		StateMachineBinding<ServiceStateMachine, string>
	{
		public ServiceStateMachineBinding()
		{
			Id(x => x.Name);

			Bind(ServiceStateMachine.OnCreated, x => x.ServiceName);
			Bind(ServiceStateMachine.OnStart, x => x.ServiceName);
			Bind(ServiceStateMachine.OnStop, x => x.ServiceName);
			Bind(ServiceStateMachine.OnReload, x => x.ServiceName);
			Bind(ServiceStateMachine.OnCreated, x => x.ServiceName);
			Bind(ServiceStateMachine.OnStopped, x => x.ServiceName);
		}
	}
}