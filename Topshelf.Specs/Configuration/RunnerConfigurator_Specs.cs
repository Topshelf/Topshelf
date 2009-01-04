namespace Topshelf.Specs.Configuration
{
    using System.ServiceProcess;
    using Internal;
    using MbUnit.Framework;
    using Microsoft.Practices.ServiceLocation;
    using Rhino.Mocks;
    using Topshelf.Configuration;

    [TestFixture]
    public class RunnerConfigurator_Specs
    {
        private RunConfiguration _runConfiguration;
        [SetUp]
        public void EstablishContext()
        {
            _runConfiguration = (RunConfiguration)RunnerConfigurator.New(x =>
            {
                x.SetDisplayName("chris");
                x.SetServiceName("chris");
                x.SetDescription("chris's pants");

                x.ConfigureService<TestService>(c =>
                {
                    c.WithName("my_service");
                    c.WhenStarted(s => s.Start());
                    c.WhenStopped(s => s.Stop());
                    c.WhenPaused(s => { });
                    c.WhenContinued(s => { });
                    //WhenRestarted (stop / start)
                });
                x.ConfigureService<TestService>(); //defaults

                x.DoNotStartAutomatically();

                x.RunAs("dru", "pass");

                //x.UseWinFormHost<MyForm>();

                x.DependsOn("ServiceName");
                x.DependencyOnMsmq();
                x.DependencyOnMsSql();
            });
        }
        //[Test]
        public void Syntax_Play()
        {
            IRunConfiguration cfg = RunnerConfigurator.New(x =>
                      {
                          x.SetDisplayName("chris");
                          x.SetServiceName("chris");
                          x.SetDescription("chris's pants");
                              
                          x.ConfigureService<TestService>(c=>
                              {
                                  c.WithName("my_service");
                                  c.WhenStarted(s => s.Start());
                                  c.WhenStopped(s => s.Stop());
                                  c.WhenPaused(s => { });
                                  c.WhenContinued(s => { });
                                  //WhenRestarted (stop / start)
                              });
                          x.ConfigureService<TestService>(); //defaults

                          x.DoNotStartAutomatically();

                          x.RunAs("dru", "pass");
                          x.RunAsLocalSystem();
                          //x.RunAs(AppConfig("username"), AppConfig("password"));
                          x.RunAsFromInteractive();

                          //x.UseWinFormHost<MyForm>();

                          x.DependsOn("ServiceName");
                          x.DependencyOnMsmq();
                          x.DependencyOnMsSql();
                      });


            //serviceCoordinator.TakeAction(args);
        }

        [Test]
        public void A_pretend_void_main()
        {
            string[] args = new string[0];
            IRunConfiguration cfg = RunnerConfigurator.New(x => { });
            //some thing parses the args
            //Dispatch(args, serviceCoordinator);
        }

        [Test]
        public void Should_depend_on_Msmq_MsSql_and_Custom()
        {
            _runConfiguration.WinServiceSettings.Dependencies
                .ShouldContain(KnownServiceNames.Msmq);

            _runConfiguration.WinServiceSettings.Dependencies
                .ShouldContain(KnownServiceNames.SqlServer);

            _runConfiguration.WinServiceSettings.Dependencies
                .ShouldContain("ServiceName");
        }

        [Test]
        public void Names_should_be_correct()
        {
            _runConfiguration.WinServiceSettings.DisplayName
                .ShouldEqual("chris");

            _runConfiguration.WinServiceSettings.ServiceName
                .ShouldEqual("chris");

            _runConfiguration.WinServiceSettings.Description
                .ShouldEqual("chris's pants");
        }

        [Test]
        public void Should_not_be_set_to_start_automatically()
        {
            _runConfiguration.WinServiceSettings.StartMode
                .ShouldEqual(ServiceStartMode.Manual);
        }

        [Test]
        public void Credentials()
        {
            _runConfiguration.Credentials.Username
                .ShouldEqual("dru");

            _runConfiguration.Credentials.Password
                .ShouldEqual("pass");

            _runConfiguration.Credentials.AccountType
                .ShouldEqual(ServiceAccount.User);
        }

        [Test]
        public void Hosted_service_configuration()
        {
            _runConfiguration.Coordinator.HostedServiceCount
                .ShouldEqual(2);

            IService service = _runConfiguration.Coordinator.GetService("my_service");

            service.Name
                .ShouldEqual("my_service");
            service.State
                .ShouldEqual(ServiceState.Stopped);
        }
    }
}