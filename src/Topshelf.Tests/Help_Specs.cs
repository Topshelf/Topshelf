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

    [TestFixture]
    public class Using_the_command_line_help_host
    {
        [Test]
        public void Should_be_able_to_add_prefix_text_to_help()
        {
            var prefix = "PhatBoyG In The House!";

            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.SetHelpTextPrefix(prefix);
                x.ApplyCommandLine("help");
            });

            Assert.IsInstanceOf<HelpHost>(host);
            var helpHost = (HelpHost)host;
            Assert.AreEqual(prefix, helpHost.PrefixText);
        }

        [Test]
        public void Should_be_requested_via_the_command_line()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<MyService>();
                x.ApplyCommandLine("help");
            });

            Assert.IsInstanceOf<HelpHost>(host);
            var helpHost = (HelpHost)host;
            Assert.AreEqual(null, helpHost.PrefixText);
        }

        [Test]
        public void Should_ignore_prefix_text_if_system_only_help_requested()
        {
            var prefix = "PhatBoyG In The House!";

            Host host = HostFactory.New(x =>
                {
                    x.Service<MyService>();
                    x.SetHelpTextPrefix(prefix);
                    x.ApplyCommandLine("help --systemonly");
                });

            Assert.IsInstanceOf<HelpHost>(host);
            var helpHost = (HelpHost)host;
            Assert.AreEqual(null, helpHost.PrefixText);
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