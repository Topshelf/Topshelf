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
	public delegate TService ServiceFactory<TService>(string name)
		where TService : class;


	public delegate TService InternalServiceFactory<TService>(string name, IServiceChannel coordinatorChannel)
		where TService : class;


	public delegate TService DescriptionServiceFactory<TService>(
		ServiceDescription description, string name, IServiceChannel coordinatorChannel)
		where TService : class;
}