// Copyright 2007-2012 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// his file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using Internal;
	using Magnum.Extensions;


    [DebuggerDisplay("{GetServiceName()}")]
	public class WindowsServiceDescription :
		ServiceDescription
	{
		public const string InstanceSeparator = "$";
		string _description;
		string _displayName;

		/// <summary>
		/// Creates a new WindowsServiceDescription using empty strings for the properties.
		/// The class is required to have names by the consumers.
		/// </summary>
		public WindowsServiceDescription()
			: this(string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Creates a new WindowsServiceDescription instance using the
		/// passed parameters.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="instanceName"></param>
		public WindowsServiceDescription([NotNull] string name, [NotNull] string instanceName)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (instanceName == null)
				throw new ArgumentNullException("instanceName");

			Name = name;
			InstanceName = instanceName;

			_displayName = "";
			_description = "";
		}

		[NotNull]
		public string Name { get; set; }

		[NotNull]
		public string DisplayName
		{
			get
			{
				string displayName = _displayName.IsNotEmpty() ? _displayName : Name;

				if (InstanceName.IsNotEmpty())
					return "{0} (Instance: {1})".FormatWith(displayName, InstanceName);

				return displayName;
			}
			set { _displayName = value; }
		}

		[NotNull]
		public string Description
		{
			get { return _description.IsEmpty() ? DisplayName : _description; }
			set { _description = value; }
		}

		[NotNull]
		public string InstanceName { get; set; }

#if NET40
		[Pure]
#endif
		public string GetServiceName()
		{
			return InstanceName.IsEmpty() ? Name : Name + InstanceSeparator + InstanceName;
		}
	}
}