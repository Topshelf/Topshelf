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
            Assert.That(script, Is.StringContaining(string.Format("exec mono {0}", _subject.InstanceName)));
        }

        [Test]
        public void Should_generate_startup_event_stanza()
        {
            var script = LinuxHostEnvironment.GenerateUpstartScript(_subject);
            
            foreach (var dependency in _subject.Dependencies)
                Assert.That(script, Is.StringContaining(dependency));

            Assert.That(script, Is.StringContaining("start on"));
        }

        [Test]
        public void Should_generate_configuration_path()
        {
            var paths = LinuxHostEnvironment.GeneratePathsForService(_subject);
            Assert.That(paths.Configuration, Is.EqualTo(string.Format("/etc/{0}.conf", _subject.ServiceName)));
        }

        [Test]
        public void Should_generate_location_directory()
        {
            // this directory is dependent on where we run the service
            // but by convention it should be in 
            // /opt/$ServiceName, so let's go for that initially while
            // hashing this out, but later we can do Assembly.GetExecutingAssembly().Location
            // or Environment.CurrentDirectory.
            var paths = LinuxHostEnvironment.GeneratePathsForService(_subject);
            Assert.That(paths.WorkingDirectory, Is.EqualTo(string.Format("/opt/{0}", _subject.ServiceName)));
        }

        [SetUp]
        public void Given_installation_options()
        {
            Host host = HostFactory.New(x =>
                {
                    x.Service<IndexerService>();
                    x.SetInstanceName("indexer");
                    x.SetServiceName("indexer-service");
                    x.DependsOn("rabbitmq");

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