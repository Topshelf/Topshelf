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
    using System;
    using System.Collections.Generic;
	using System.Configuration;
	using System.ServiceProcess;
	using Extensions;
	using WindowsServiceCode;


	public class WinServiceSettings
	{
		public WinServiceSettings()
		{
			ServiceRecoveryOptions = new ServiceRecoveryOptions();
		
			StartMode = ServiceStartMode.Automatic;
			Dependencies = new List<string>();

			Description = string.Empty;
			DisplayName = string.Empty;
			ServiceName = new ServiceName(string.Empty);
		}

		public ServiceStartMode StartMode { get; set; }
		public ServiceName ServiceName { get; set; }
		public string DisplayName { private get; set; }
		public string Description { get; set; }
		public Credentials Credentials { get; set; }
		public ServiceRecoveryOptions ServiceRecoveryOptions { get; set; }
        public Action AfterInstallAction { get; set; }
        public Action AfterUninstallAction { get; set; }

		public string FullDisplayName
		{
			get
			{
				return string.IsNullOrEmpty(ServiceName.InstanceName)
				       	? DisplayName
				       	: "{0} (Instance: {1})".FormatWith(DisplayName, ServiceName.InstanceName);
			}
		}

		public List<string> Dependencies { get; set; }

		public static WinServiceSettings DotNetConfig
		{
			get
			{
				var settings = new WinServiceSettings
					{
						ServiceName = new ServiceName(ConfigurationManager.AppSettings["serviceName"]),
						DisplayName = ConfigurationManager.AppSettings["displayName"],
						Description = ConfigurationManager.AppSettings["description"],
					};

				settings.Dependencies.AddRange(ConfigurationManager.AppSettings["dependencies"].Split(','));
				return settings;
			}
		}

		public string ImagePath
		{
			get { return ServiceName.InstanceName == null ? " " : " -instance:{0}".FormatWith(ServiceName.InstanceName); }
		}

		public static WinServiceSettings Custom(string serviceName, string displayName, string description,
		                                        params string[] dependencies)
		{
			var settings = new WinServiceSettings
				{
					ServiceName = new ServiceName(serviceName),
					DisplayName = displayName,
					Description = description,
				};
			settings.Dependencies.AddRange(dependencies);
			return settings;
		}
	}
}