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
namespace Topshelf.Windows
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Reflection;
	using System.Security.Principal;
	using log4net;
	using Magnum.CommandLineParser;


	public static class WindowsUserAccessControl
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Windows.WindowsUserAccessControl");

		public static bool IsAdministrator
		{
			get
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();

				if (null != identity)
				{
					var principal = new WindowsPrincipal(identity);
					return principal.IsInRole(WindowsBuiltInRole.Administrator);
				}

				return false;
			}
		}

		public static bool RerunAsAdministrator()
		{
			if (Environment.OSVersion.Version.Major == 6)
			{
				var startInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, CommandLine.GetUnparsedCommandLine())
					{
						Verb = "runas",
						UseShellExecute = true,
						CreateNoWindow = true
					};

				try
				{
					LogManager.Shutdown();

					Process process = Process.Start(startInfo);
					process.WaitForExit();

					return true;
				}
				catch (Win32Exception ex)
				{
					_log.Debug("Process Start Exception", ex);
				}
			}

			return false;
		}
	}
}