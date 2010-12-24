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
namespace Topshelf
{
	using System;
	using System.IO;
	using Commands;
	using Configuration;
	using log4net;


	/// <summary>
	/// Entry point into the Host infrastructure
	/// </summary>
	public static class Runner
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Runner");

		static Runner()
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
		}

		/// <summary>
		/// Go go gadget
		/// </summary>
		public static void Host(RunConfiguration configuration, string[] args)
		{
			var commandLine = string.Join(" ", args);
			if (commandLine.Length > 0)
				_log.DebugFormat("Command Line Arguments: '{0}'", commandLine);


			Command command = new CommandLineArguments(configuration)
				.GetCommand(commandLine);

			command.Execute();
		}
	}
}