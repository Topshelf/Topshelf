// Copyright 2007-2008 The Apache Software Foundation.
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
    using Magnum.TestFramework;
    using NUnit.Framework;
    using Topshelf.Configuration;

    [Scenario]
    public class Given_a_command_line
    {
        protected string CommandLine
        {
            set
            {
                Arguments = TopshelfArgumentParser.Parse(value);
            }
        }
        protected TopshelfArguments Arguments { get; set; }
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
            Arguments.ActionName.ShouldEqual(ServiceActionNames.Run);
        }
        
        [Then]
        public void Instance_should_be_empty()
        {
            Arguments.Instance.ShouldBeEmpty();
        }
    }

    [Scenario, Explicit("Not Yet Implemented")]
    public class Given_service_install_command_line :
        Given_a_command_line
    {
        [Given]
        public void Service_install_command_line()
        {
            CommandLine = "service install";
        }

        [Then]
        public void Action_should_be_install()
        {
            Arguments.ActionName.ShouldEqual(ServiceActionNames.Install);
        }

        [Then]
        public void Instance_should_be_empty()
        {
            Arguments.Instance.ShouldBeEmpty();
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
            Arguments.ActionName.ShouldEqual(ServiceActionNames.Install);
        }

        [Then]
        public void Instance_should_be_bob()
        {
            Arguments.Instance.ShouldEqual("bob");
        }
    }
}