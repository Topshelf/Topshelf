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
namespace Topshelf.Specs
{
	using Commands;
	using Magnum.TestFramework;
    using NUnit.Framework;
    using Topshelf.Configuration;
	using Topshelf.Configuration.Dsl;


	[Scenario]
    public class Given_a_command_line
    {
		protected RunConfiguration Configuration { get; set; }

    	public Given_a_command_line()
    	{
    		Configuration = RunnerConfigurator.New(x => { });
    	}

    	protected string CommandLine
        {
            set
            {
            	Command = new CommandLineArguments(Configuration).GetCommand(value);
            }
        }

        protected Command Command { get; set; }
    }


    [Scenario]
    public class Given_no_command_line :
        Given_a_command_line
    {
        [Given]
        public void An_empty_command_line()
        {
            CommandLine = string.Empty;
        }

        [Then]
        public void Action_should_be_run()
        {
            Command.Name.ShouldEqual(ServiceActionNames.Run);
        }

        [Then]
        public void Instance_should_be_empty()
        {
            string.IsNullOrEmpty(Configuration.WinServiceSettings.ServiceName.InstanceName).ShouldBeTrue();
        }
    }


    [Scenario]
    public class Given_a_dash_parameter :
        Given_a_command_line
    {
        [Given]
        public void Dash_something_or_other_command_line()
        {
            CommandLine = "-SomethingOrOther";
        }

        [Then]
        public void Action_should_be_run()
        {
			Command.Name.ShouldEqual(ServiceActionNames.Run);
        }
    }


    [Scenario]
    [Explicit("Not Yet Implemented")]
    public class Given_service_install_command_line :
        Given_a_command_line
    {
        [Given]
        public void Service_install_command_line()
        {
            CommandLine = "install";
        }

        [Then]
        public void Action_should_be_install()
        {
            Command.Name.ShouldEqual(ServiceActionNames.Install);
        }

        [Then]
        public void Instance_should_be_empty()
        {
            string.IsNullOrEmpty(Configuration.WinServiceSettings.ServiceName.InstanceName).ShouldBeTrue();
        }
    }


    [Scenario]
    public class Give_install_with_an_instance :
        Given_a_command_line
    {
        [Given]
        public void An_empty_command_line()
        {
            CommandLine = "install /instance:bob";
        }

        [Then]
        public void Action_should_be_install()
        {
            Command.Name.ShouldEqual(ServiceActionNames.Install);
        }

        [Then]
        public void Instance_should_be_bob()
        {
            Configuration.WinServiceSettings.ServiceName.InstanceName.ShouldEqual("bob");
        }
    }
}