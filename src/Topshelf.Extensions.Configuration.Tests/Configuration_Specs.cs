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
namespace Topshelf.Extensions.Configuration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hosts;
    using Microsoft.Extensions.Configuration;
    using NUnit.Framework;
    using Runtime;
    using Topshelf.Configuration;
    using Topshelf.HostConfigurators;
    using Topshelf.Options;
    using Topshelf.Runtime.Windows;

    [TestFixture]
    public class Passing_install
    {
        [Test]
        public void Should_create_an_install_host_with_service_name()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:ServiceName", "Joe" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.Settings.Name);
            Assert.AreEqual("Joe", installHost.Settings.ServiceName);
        }

        [Test]
        public void Should_create_an_install_host_with_display_name()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:DisplayName", "Joe" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.Settings.DisplayName);
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_and_instance_name()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:DisplayName", "Joe" },
                        { "topshelf:Instance", "42" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe (Instance: 42)", installHost.Settings.DisplayName);
        }

        [Test]
        public void Should_create_an_install_host_with_display_name_with_instance_name()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:DisplayName", "Joe (Instance: 42)" },
                        { "topshelf:Instance", "42" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe (Instance: 42)", installHost.Settings.DisplayName);
        }

        [Test]
        public void Should_create_an_install_host_with_description()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:Description", "Joe is good" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe is good", installHost.Settings.Description);
        }

        [Test]
        public void Should_create_an_install_host_with_service_name_and_instance_name()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:ServiceName", "Joe" },
                        { "topshelf:Instance", "42" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.Settings.Name);
            Assert.AreEqual("42", installHost.Settings.InstanceName);
            Assert.AreEqual("Joe$42", installHost.Settings.ServiceName);
        }

        [Test]
        public void Should_create_and_install_host_with_service_name_containing_space()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:ServiceName", "Joe's Service" },
                        { "topshelf:Instance", "42" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe's Service", installHost.Settings.Name);
            Assert.AreEqual("42", installHost.Settings.InstanceName);
            Assert.AreEqual("Joe's Service$42", installHost.Settings.ServiceName);
        }

        [Test]
        public void Should_create_an_install_host_to_start_automatically()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:StartMode", "auto" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.Automatic, installHost.InstallSettings.StartMode);
        }

        [Test]
        public void Should_create_an_install_host_to_start_manually()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:StartMode", "Manual" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.Manual, installHost.InstallSettings.StartMode);
        }

        [Test]
        public void Should_create_an_install_host_to_set_disabled()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:StartMode", "Disabled" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.Disabled, installHost.InstallSettings.StartMode);
        }

#if !NET35
        [Test]
        public void Should_create_an_install_host_to_start_delayed()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:StartMode", "Delayed" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(HostStartMode.AutomaticDelayed, installHost.InstallSettings.StartMode);
        }
#endif

        [Test]
        public void Should_require_password_option_when_specifying_username()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:Account:Username", "Joe" },
                    })
                .Build()
                .GetSection("topshelf");

            Assert.Throws<Topshelf.HostConfigurationException>(() =>
            {
                Host host = HostFactory.New(x =>
                    {
                        x.Service<MyService>();
                        x.ApplyConfiguration(configuration);
                        x.ApplyCommandLine("install");
                    });
            });
        }

        [Test]
        public void Will_allow_blank_password_when_specifying_username()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:Account:Username", "Joe" },
                        { "topshelf:Account:Password", "" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("Joe", installHost.InstallSettings.Credentials.Username);
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Password);
        }

        [Test]
        public void Should_create_a_service_that_runs_as_local_system()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:Account", "LocalSystem" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Username);
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Password);
        }

        [Test]
        public void Should_create_a_service_that_runs_as_local_service()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:Account", "LocalService" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Username);
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Password);
        }

        [Test]
        public void Should_create_a_service_that_runs_as_network_service()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:Account", "NetworkService" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Username);
            Assert.AreEqual("", installHost.InstallSettings.Credentials.Password);
        }

        [Test]
        public void Should_create_a_service_that_has_start_timeout()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:StartTimeout", "123" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(TimeSpan.FromSeconds(123), installHost.InstallSettings.StartTimeOut);
        }

        [Test]
        public void Should_create_a_service_that_has_stopt_timeout()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:StopTimeout", "123" },
                    })
                .Build()
                .GetSection("topshelf");

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.ApplyConfiguration(configuration);
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);
            var installHost = (InstallHost)host;
            Assert.AreEqual(TimeSpan.FromSeconds(123), installHost.InstallSettings.StopTimeOut);
        }

        [Test]
        public void Should_create_a_service_with_recovery_options()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "topshelf:ServiceRecovery:RecoverOnCrashOnly", "true" },
                        { "topshelf:ServiceRecovery:ResetPeriod", "123" },
                        { "topshelf:ServiceRecovery:RecoveryActions:0:Type", "RestartService" },
                        { "topshelf:ServiceRecovery:RecoveryActions:0:Delay", "1234" },
                        { "topshelf:ServiceRecovery:RecoveryActions:1:Type", "RestartSystem" },
                        { "topshelf:ServiceRecovery:RecoveryActions:1:Delay", "4567" },
                        { "topshelf:ServiceRecovery:RecoveryActions:1:Message", "message" },
                        { "topshelf:ServiceRecovery:RecoveryActions:2:Type", "RunProgram" },
                        { "topshelf:ServiceRecovery:RecoveryActions:2:Delay", "8901" },
                        { "topshelf:ServiceRecovery:RecoveryActions:2:Message", "command" },
                    })
                .Build()
                .GetSection("topshelf");

            var parsedConfiguration = configuration.Parse().First();

            Assert.IsInstanceOf<ServiceRecoveryOption>(parsedConfiguration);
            var serviceRecoveryOptions = typeof(ServiceRecoveryOption)
                .GetField("serviceRecoveryOptions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(parsedConfiguration) as ServiceRecoveryOptions;

            Assert.AreEqual(true, serviceRecoveryOptions.RecoverOnCrashOnly);
            Assert.AreEqual(123, serviceRecoveryOptions.ResetPeriod);

            var actions = serviceRecoveryOptions.Actions.ToArray();

            Assert.IsInstanceOf<RestartServiceRecoveryAction>(actions[0]);
            var restartServiceRecoveryAction = actions[0] as RestartServiceRecoveryAction;
            Assert.AreEqual(TimeSpan.FromMinutes(1234).TotalMilliseconds, restartServiceRecoveryAction.Delay);

            Assert.IsInstanceOf<RestartSystemRecoveryAction>(actions[1]);
            var restartSystemRecoveryAction = actions[1] as RestartSystemRecoveryAction;
            Assert.AreEqual(TimeSpan.FromMinutes(4567).TotalMilliseconds, restartSystemRecoveryAction.Delay);
            Assert.AreEqual("message", restartSystemRecoveryAction.RestartMessage);

            Assert.IsInstanceOf<RunProgramRecoveryAction>(actions[2]);
            var runProgramRecoveryAction = actions[2] as RunProgramRecoveryAction;
            Assert.AreEqual(TimeSpan.FromMinutes(8901).TotalMilliseconds, runProgramRecoveryAction.Delay);
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

        class MyHostConfigurator : HostConfigurator
        {
            public HostBuilderConfigurator Configuratior { get; private set; }

            public UnhandledExceptionPolicyCode UnhandledExceptionPolicy => throw new NotImplementedException();

            UnhandledExceptionPolicyCode HostConfigurator.UnhandledExceptionPolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void AddCommandLineDefinition(string name, Action<string> callback)
            {
                throw new NotImplementedException();
            }

            public void AddCommandLineSwitch(string name, Action<bool> callback)
            {
                throw new NotImplementedException();
            }

            public void AddConfigurator(HostBuilderConfigurator configurator)
            {
                this.Configuratior = configurator;
            }

            public void ApplyCommandLine()
            {
                throw new NotImplementedException();
            }

            public void ApplyCommandLine(string commandLine)
            {
                throw new NotImplementedException();
            }

            public void EnablePauseAndContinue()
            {
                throw new NotImplementedException();
            }

            public void EnableSessionChanged()
            {
                throw new NotImplementedException();
            }

            public void EnablePowerEvents()
            {
                throw new NotImplementedException();
            }

            public void EnableShutdown()
            {
                throw new NotImplementedException();
            }

            public void OnException(Action<Exception> callback)
            {
                throw new NotImplementedException();
            }

            public void SetDescription(string description)
            {
                throw new NotImplementedException();
            }

            public void SetDisplayName(string name)
            {
                throw new NotImplementedException();
            }

            public void SetInstanceName(string instanceName)
            {
                throw new NotImplementedException();
            }

            public void SetServiceName(string name)
            {
                throw new NotImplementedException();
            }

            public void SetStartTimeout(TimeSpan startTimeOut)
            {
                throw new NotImplementedException();
            }

            public void SetStopTimeout(TimeSpan stopTimeOut)
            {
                throw new NotImplementedException();
            }

            public void UseEnvironmentBuilder(EnvironmentBuilderFactory environmentBuilderFactory)
            {
                throw new NotImplementedException();
            }

            public void UseHostBuilder(HostBuilderFactory hostBuilderFactory)
            {
                throw new NotImplementedException();
            }

            public void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory)
            {
                throw new NotImplementedException();
            }
        }
    }
}
