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
namespace Topshelf
{
	using System.Collections.Generic;
	using System.Linq;
	using Commands;
	using Configuration;
	using Magnum.CommandLineParser;
	using Magnum.Monads.Parser;


	public class CommandLineArguments
	{
		readonly RunConfiguration _configuration;
		string _commandLine;

		public CommandLineArguments(RunConfiguration configuration)
		{
			_configuration = configuration;
		}

		public Command GetCommand(string commandLine)
		{
			_commandLine = commandLine;

			Command command = CommandLine.Parse<Command>(commandLine, InitializeCommandLineParser)
				.DefaultIfEmpty(new RunCommand(_configuration.Coordinator, _configuration.WinServiceSettings.ServiceName, null))
				.First();

			return command;
		}

		void InitializeCommandLineParser(ICommandLineElementParser<Command> x)
		{
		    Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> definitions =
				(from username in x.Definition("username") select username)
					.Or(from password in x.Definition("password") select password)
					.Or(from instance in x.Definition("instance") select instance);

			Parser<IEnumerable<ICommandLineElement>, ISwitchElement> switches =
				(from username in x.Switch("username") select username)
					.Or(from password in x.Switch("password") select password)
					.Or(from password in x.Switch("password") select password)
					.Or(from password in x.Switch("password") select password);

			x.Add(from arg in x.Argument("install")
			      from instance in definitions.Optional("instance", null)
			      from username in definitions.Optional("username", null)
			      from password in definitions.Optional("password", null)
			      select (Command)new InstallCommand(_configuration.WinServiceSettings, _commandLine, instance.Value, username.Value, password.Value));

			x.Add(from arg in x.Argument("uninstall")
			      from instance in definitions.Optional("instance", null)
			      select (Command)new UninstallCommand(_configuration.WinServiceSettings, _commandLine, instance.Value));

			x.Add(from arg in x.Argument("run")
                  from instance in definitions.Optional("instance",null)
			      select (Command)new RunCommand(_configuration.Coordinator, _configuration.WinServiceSettings.ServiceName, instance.Value));

            x.Add(from arg in x.Definition("instance")
                  select (Command)new RunCommand(_configuration.Coordinator, _configuration.WinServiceSettings.ServiceName, arg.Value));
		}
	}
}