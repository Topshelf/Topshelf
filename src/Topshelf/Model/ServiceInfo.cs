// Copyright 2007-2011 The Apache Software Foundation.
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
	public class ServiceInfo
	{
		public ServiceInfo(string name, string currentState, string serviceType)
		{
			Name = name;
			CurrentState = currentState;
			ServiceType = serviceType;
		}

		protected ServiceInfo()
		{
		}

		public string Name { get; private set; }
		public string CurrentState { get; private set; }
		public string ServiceType { get; private set; }
	}
}