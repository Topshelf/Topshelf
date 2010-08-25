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
namespace Topshelf.Messages
{
	using System;
	using System.Reflection;
	using Shelving;


	public class CreateShelfService
	{
		public CreateShelfService(string serviceName, ShelfType shelfType)
			: this(serviceName, shelfType, null)
		{
		}

		public CreateShelfService(string serviceName, ShelfType shelfType, Type bootstrapperType)
			: this(serviceName, shelfType, bootstrapperType, new AssemblyName[] {})
		{
		}

		public CreateShelfService(string serviceName, ShelfType shelfType, Type bootstrapperType, AssemblyName[] assemblyNames)
		{
			ServiceName = serviceName;
			ShelfType = shelfType;
			BootstrapperType = bootstrapperType;
			AssemblyNames = assemblyNames;
		}

		protected CreateShelfService()
		{
		}

		public string ServiceName { get; private set; }

		public ShelfType ShelfType { get; private set; }
		public Type BootstrapperType { get; private set; }
		public AssemblyName[] AssemblyNames { get; private set; }
	}
}