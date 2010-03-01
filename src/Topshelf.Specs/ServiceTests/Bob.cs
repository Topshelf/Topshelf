namespace Topshelf.Specs.ServiceTests
{
    using System;
    using NUnit.Framework;
    using Topshelf.Configuration;
    using Topshelf.Configuration.Dsl;

    [Category("Integration")]
    [Explicit] //don't just run these for shits and giggles yet
    [TestFixture]
    public class Bob
    {
        RunConfiguration _configuration;

        [TestFixtureSetUp]
        public void EstablishContext()
        {
            _configuration = RunnerConfigurator.New(x =>
            {
                x.SetDescription("topshelf test installation");
                x.SetDisplayName("TOPSHELF-TEST");
                x.SetServiceName("TOPSHELF-TEST");

                x.RunAsLocalSystem();

                x.ConfigureServiceInIsolation<TestInstall>(s =>
                {
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
            });
        }

        [Test]
        public void Should_Install()
        {
            var commandline = new[] { "service", "-install" };
            Runner.Host(_configuration, commandline);

            //assert the service installed
        }

        [Test]
        public void Should_Uninstall()
        {
            var commandline = new[] { "service", "-uninstall" };

        }
    }

    public class TestInstall
    {
        public void Start()
        {
        }
        public void Stop()
        {

        }
    }
}