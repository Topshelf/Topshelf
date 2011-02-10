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
	using Hosts;
	using Magnum.TestFramework;


	[Scenario]
    public class Given_no_command_line
    {
    	Host _host;

    	[Given]
        public void An_empty_command_line()
    	{
    		_host = HostFactory.New(x =>
    			{
    				x.ApplyCommandLine("");
    			});
    	}

    	[Then]
        public void Action_should_be_run()
    	{
    		_host.ShouldBeAnInstanceOf<ConsoleRunHost>();
        }
    }


    [Scenario]
    public class Given_a_dash_parameter
    {
    	Host _host;

    	[Given]
        public void Dash_something_or_other_command_line()
        {
			_host = HostFactory.New(x =>
			{
				x.ApplyCommandLine("-unknownArgument");
			});
		}

        [Then]
        public void Action_should_be_run()
        {
        	_host.ShouldBeAnInstanceOf<ConsoleRunHost>();
        }
    }


    [Scenario]
    public class Given_service_install_command_line
    {
    	Host _host;

    	[Given]
        public void Service_install_command_line()
        {
			_host = HostFactory.New(x =>
			{
				x.ApplyCommandLine("install");
			});
		}

        [Then]
        public void Action_should_be_install()
        {
        	_host.ShouldBeAnInstanceOf<InstallHost>();
        }
    }


    [Scenario]
    public class Give_install_with_an_instance
    {
    	Host _host;

    	[Given]
        public void An_empty_command_line()
        {
			_host = HostFactory.New(x =>
				{
					x.SetServiceName("test");
				x.ApplyCommandLine("install -instance:bob");
			});
		}

        [Then]
        public void Action_should_be_install()
        {
        	_host.ShouldBeAnInstanceOf<InstallHost>();
        }

        [Then]
        public void Instance_should_be_bob()
        {
        	((InstallHost)_host).Description.GetServiceName().ShouldEqual("test$bob");
        }
    }
}