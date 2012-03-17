//  Copyright 2007-2012 The Apache Software Foundation.
//   
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.
namespace Topshelf.Specs.ServiceTests
{
    using System;
    using System.Linq;
    using System.ServiceProcess;
    using NUnit.Framework;


    [Category("Integration")]
	[Explicit] //don't just run these for shits and giggles yet
	[TestFixture]
	public class ServiceInstallAndUninstallOnWindows
	{
	    private string serviceName = "TOPSHELF-TEST";

		[Test]
		public void Should_Install()
		{
            Assert.IsFalse(IsServiceInstalled(serviceName), serviceName + " was already installed. Can't install.");
			var commandline = "install";
            Action<HostConfigurators.HostConfigurator> config = c =>
            {
                c.ApplyCommandLine(commandline);
                _configuration(c);
            };
			HostFactory.Run(_configuration);
            Assert.IsTrue(IsServiceInstalled(serviceName), serviceName + " was not installed using HostFactory.Run.");

		}

        [Test]
        public void Should_Uninstall()
        {
            Assert.IsTrue(IsServiceInstalled(serviceName), serviceName + " was not already installed. Can't uninstall.");
            var commandline = "uninstall";
            Action<HostConfigurators.HostConfigurator> config = c =>
                {
                    c.ApplyCommandLine(commandline);
                    _configuration(c);
                };
            HostFactory.Run(config);
            Assert.IsFalse(IsServiceInstalled(serviceName), serviceName + " was not uninstalled using HostFactory.Run");
        }

	    Action<HostConfigurators.HostConfigurator> _configuration;

		[TestFixtureSetUp]
		public void EstablishContext()
		{
			_configuration = x =>
				{
					x.SetDescription("topshelf test installation");
					x.SetDisplayName("TOPSHELF-TEST");
                    x.SetServiceName(serviceName);

					x.RunAsLocalSystem();
					x.Service<TestInstall>(s =>
					    {
					        s.ConstructUsing(() => new TestInstall());
							s.WhenStarted(tc => tc.Start());
							s.WhenStopped(tc => tc.Stop());
						});
				};
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