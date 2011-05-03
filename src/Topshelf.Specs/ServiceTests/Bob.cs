namespace Topshelf.Specs.ServiceTests
{
    using System.Linq;
    using System.ServiceProcess;
    using NUnit.Framework;
	using Topshelf.Configuration;
	using Topshelf.Configuration.Dsl;


	[Category("Integration")]
	[Explicit] //don't just run these for shits and giggles yet
	[TestFixture]
	public class Bob
	{
	    private string serviceName = "TOPSHELF-TEST";

		[Test]
		public void Should_Install()
		{
            Assert.IsFalse(IsServiceInstalled(serviceName), serviceName + " was already installed. Can't install.");
			var commandline = new[] {"service", "install"};
			Runner.Host(_configuration, commandline);
            Assert.IsTrue(IsServiceInstalled(serviceName), serviceName + " was not installed using Runner.Host.");

		}

		[Test]
		public void Should_Uninstall()
		{
            Assert.IsTrue(IsServiceInstalled(serviceName), serviceName + " was not already installed. Can't uninstall.");
            var commandline = new[] { "service", "uninstall" };
            Runner.Host(_configuration, commandline);
            Assert.IsFalse(IsServiceInstalled(serviceName), serviceName + " was not uninstalled using Runner.Host");
		}

		RunConfiguration _configuration;

		[TestFixtureSetUp]
		public void EstablishContext()
		{
			_configuration = RunnerConfigurator.New(x =>
				{
					x.SetDescription("topshelf test installation");
					x.SetDisplayName("TOPSHELF-TEST");
                    x.SetServiceName(serviceName);

					x.RunAsLocalSystem();

					x.ConfigureService<TestInstall>(s =>
						{
							s.WhenStarted(tc => tc.Start());
							s.WhenStopped(tc => tc.Stop());
						});
				});
		}

        private bool IsServiceInstalled(string serviceName)
        {
            var services = ServiceController.GetServices().ToList();

            return services.Where(x => x.ServiceName == serviceName).Count() > 0;
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