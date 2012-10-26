using System;
using NUnit.Framework;
using Topshelf.Hosts;
using Topshelf.Runtime;
using Topshelf.Runtime.Linux;

namespace Topshelf.Tests
{
#if LINUX
    public class Using_linux_upstart_generator
    {
        InstallHostSettings _subject;

        /*
         * Sample:
         * 
         * 
start on virtual-filesystems
stop on runlevel [06]

respawn
respawn limit 5 30

env LISTEN_PORT='8081'
chdir /opt/indexer
setuid myservice-user
setgid ddd-services
console log

script
        exec mono indexer -f /etc/indexer.conf
end script

         * 
         */

        [Test]
        public void Should_generate_simple_exec_stanza()
        {
            var script = LinuxHostEnvironment.GenerateUpstartScript(_subject);
            Assert.That(script, Is.StringContaining("exec mono indexer"));
        }

        [SetUp]
        public void Given_installation_options()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<IndexerService>();
                    x.SetInstanceName("indexer");
                    x.ApplyCommandLine("install");
                });

            Assert.IsInstanceOf<InstallHost>(host);

            _subject = ((InstallHost)host).InstallSettings;
        }

        class IndexerService : ServiceControl
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
#endif
}