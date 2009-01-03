namespace Topshelf.Specs.Configuration
{
    using System.ServiceProcess;
    using MbUnit.Framework;
    using Microsoft.Practices.ServiceLocation;
    using Rhino.Mocks;
    using Topshelf.Configuration;

    [TestFixture]
    public class HostConfigurator_Specs
    {
        private ServiceCoordinator _serviceCoordinator;
        [SetUp]
        public void EstablishContext()
        {
            _serviceCoordinator = (ServiceCoordinator)HostConfigurator.New(x =>
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
            IServiceCoordinator serviceCoordinator = HostConfigurator.New(x =>
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
            IServiceCoordinator serviceCoordinator = HostConfigurator.New(x => { });
            //some thing parses the args
            //Dispatch(args, serviceCoordinator);
        }

        [Test]
        public void Should_depend_on_Msmq_MsSql_and_Custom()
        {
            _serviceCoordinator.WinServiceSettings.Dependencies
                .ShouldContain(KnownServiceNames.Msmq);

            _serviceCoordinator.WinServiceSettings.Dependencies
                .ShouldContain(KnownServiceNames.SqlServer);

            _serviceCoordinator.WinServiceSettings.Dependencies
                .ShouldContain("ServiceName");
        }

        [Test]
        public void Names_should_be_correct()
        {
            _serviceCoordinator.WinServiceSettings.DisplayName
                .ShouldEqual("chris");

            _serviceCoordinator.WinServiceSettings.ServiceName
                .ShouldEqual("chris");

            _serviceCoordinator.WinServiceSettings.Description
                .ShouldEqual("chris's pants");
        }

        [Test]
        public void Should_not_be_set_to_start_automatically()
        {
            _serviceCoordinator.WinServiceSettings.StartMode
                .ShouldEqual(ServiceStartMode.Manual);
        }

        [Test]
        public void Credentials()
        {
            _serviceCoordinator.Credentials.Username
                .ShouldEqual("dru");

            _serviceCoordinator.Credentials.Password
                .ShouldEqual("pass");

            _serviceCoordinator.Credentials.AccountType
                .ShouldEqual(ServiceAccount.User);
        }

        [Test]
        public void Hosted_service_configuration()
        {
            _serviceCoordinator.HostedServiceCount
                .ShouldEqual(2);

            IService service = _serviceCoordinator.GetService("my_service");

            service.Name
                .ShouldEqual("my_service");
            service.State
                .ShouldEqual(ServiceState.Stopped);
        }

    }
}