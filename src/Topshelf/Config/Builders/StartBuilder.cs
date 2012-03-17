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
namespace Topshelf.Builders
{
	using System;
	using Hosts;
	using Internal;


	public class StartBuilder :
		Builder
	{
		HostBuilder _builder;

		public StartBuilder(HostBuilder builder) : base(builder.Description)
		{
			_builder = null;
			builder.Match<InstallBuilder>(x => { _builder = builder; });
		}

		public override Host Build()
		{
			if (_builder != null)
			{
				Host wrappedHost = _builder.Build();

				return new StartHost(Description, wrappedHost);
			}

			return new StartHost(Description);
		}
	}
}