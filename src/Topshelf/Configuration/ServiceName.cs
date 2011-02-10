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
namespace Topshelf.Configuration
{
    using System.Diagnostics;
    using Magnum.Extensions;


	[DebuggerDisplay("{FullName}")]
	public class ServiceName
	{
		const string _instanceChar = "$";
		readonly string _instanceName;
		readonly string _name;

		public ServiceName(string name)
			: this(name, null)
		{
		}

		public ServiceName(string name, string instanceName)
		{
			_name = name;
			_instanceName = instanceName;
		}

		public string Name
		{
			get { return _name; }
		}

		public string InstanceName
		{
			get { return _instanceName; }
		}

		public string FullName
		{
			get
			{
				return string.IsNullOrEmpty(_instanceName)
				       	? _name
				       	: "{0}{1}{2}".FormatWith(_name, _instanceChar, _instanceName);
			}
		}
	}
}