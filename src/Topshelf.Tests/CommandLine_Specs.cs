// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Tests
{
    using System;
    using Hosts;
    using NUnit.Framework;
    using Runtime;

    [TestFixture]
    public class Passing_install
    {
        [Test]
        public void Should_create_an_install_host()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
        }

        [Test]
        public void Should_throw_an_exception_on_an_invalid_command_line()
        {
            var exception = Assert.Throws<HostConfigurationException>(() =>
                {
                    HostFactory.New(x =>
                    {
                        x.Service<MyService>();
                        x.ApplyCommandLine("explode");
                    });
                    
                });

            Assert.IsTrue(exception.Message.Contains("explode"));
        }

        [Test]
        public void Should_create_an_install_host_with_service_name()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyCommandLine("install -servicename \"Joe\"");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.Settings.Name);
            Assert.AreEqual("Joe", installHost.Settings.ServiceName);
        }

        [Test]
        public void Should_create_an_install_host_with_display_name()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyCommandLine("install -displayname \"Joe\"");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.Settings.DisplayName);
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_and_instance_name()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname \"Joe\" -instance \"42\"");
            });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe (Instance: 42)", installHost.Settings.DisplayName);
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_with_instance_name()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install -displayname \"Joe (Instance: 42)\" -instance \"42\"");
            });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe (Instance: 42)", installHost.Settings.DisplayName);
        }

        [Test]
        public void Should_create_an_install_host_with_description()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyCommandLine("install -description \"Joe is good\"");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe is good", installHost.Settings.Description);
        }

        [Test]
        public void Should_create_an_install_host_with_service_name_and_instance_name()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyCommandLine("install -servicename \"Joe\" -instance \"42\"");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.Settings.Name);
            Assert.AreEqual("42", installHost.Settings.InstanceName);
            Assert.AreEqual("Joe$42", installHost.Settings.ServiceName);
        }

        [Test]
        public void Should_create_an_install_host_to_start_automatically()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --autostart");
            });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.Automatic, installHost.InstallSettings.StartMode);
        }

        [Test]
        public void Should_create_an_install_host_to_start_manually()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --manual");
            });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.Manual, installHost.InstallSettings.StartMode);
        }

        [Test]
        public void Should_create_an_install_host_to_set_disabled()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --disabled");
            });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.Disabled, installHost.InstallSettings.StartMode);
        }

#if !NET35
        [Test]
        public void Should_create_an_install_host_to_start_delayed()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("install --delayed");
            });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.AutomaticDelayed, installHost.InstallSettings.StartMode);
        }
#endif
        [Test]
        public void Should_create_an_uninstall_host()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("uninstall");
            });

            Assert.IsInstanceOf<UninstallHost>(host);
        }

        [Test]
        public void Should_create_a_start_host()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("start");
            });

            Assert.IsInstanceOf<StartHost>(host);
        }

        [Test]
        public void Should_create_a_stop_host()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("stop");
            });

            Assert.IsInstanceOf<StopHost>(host);
        }

        [Test]
        public void Extensible_the_command_line_should_be_yes()
        {
            bool isSuperfly = false;
            
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();

                    x.AddCommandLineSwitch("superfly", v => isSuperfly = v);

                    x.ApplyCommandLine("--superfly");
                });

            Assert.IsTrue(isSuperfly);
        }

        [Test]
        public void Need_to_handle_crazy_special_characters_in_arguments()
        {
            string password = null;
            
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();

                    x.AddCommandLineDefinition("password", v => password = v);

                    x.ApplyCommandLine("-password:abc123!@#=$%^&*()-+");
                });

            Assert.AreEqual("abc123!@#=$%^&*()-+", password);
        }

        [Test]
        public void Need_to_handle_crazy_special_characters_in_argument()
        {
            string password = null;
            
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();

                    x.AddCommandLineDefinition("password", v => password = v);

                    x.ApplyCommandLine("-password \"abc123=:,.<>/?;!@#$%^&*()-+\"");
                });

            Assert.AreEqual("abc123=:,.<>/?;!@#$%^&*()-+", password);
        }

        [Test]
        public void Extensible_the_command_line_should_be_yet_again()
        {
            string volumeLevel = null;
            
            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();

                    x.AddCommandLineDefinition("volumeLevel", v => volumeLevel = v);

                    x.ApplyCommandLine("-volumeLevel:11");
                });

            Assert.AreEqual("11", volumeLevel);
        }

        class MyService : ServiceControl
        {
            public bool Start(HostControl hostControl)
            {
                throw new NotImplementedException();
            }

            public bool Stop(HostControl hostControl)
            {
                throw new NotImplementedException();
            }

            public bool Pause(HostControl hostControl)
            {
                throw new NotImplementedException();
            }

            public bool Continue(HostControl hostControl)
            {
                throw new NotImplementedException();
            }
        }
    }
}