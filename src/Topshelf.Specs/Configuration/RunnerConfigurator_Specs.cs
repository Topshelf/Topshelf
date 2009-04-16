namespace Topshelf.Specs.Configuration
{
    using System;
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
            IServiceLocator sl = MockRepository.GenerateStub<IServiceLocator>();
            TestService s1 = new TestService();
            TestService s2 = new TestService();
            sl.Stub(x => x.GetInstance<TestService>("my_service")).Return(s1);
            sl.Stub(x => x.GetInstance<TestService>("my_service2")).Return(s2);

            _runConfiguration = (RunConfiguration)RunnerConfigurator.New(x =>
            {
                x.SetDisplayName("chris");
                x.SetServiceName("chris");
                x.SetDescription("chris's pants");

                x.ConfigureService<TestService>("my_service", c =>
                {
                    c.WhenStarted(s => s.Start());
                    c.WhenStopped(s => s.Stop());
                    c.WhenPaused(s => { });
                    c.WhenContinued(s => { });
                    c.CreateServiceLocator(()=>sl);
                });

                //needs to moved to a custom area for testing
                //x.ConfigureServiceInIsolation<TestService>("my_service2", c=>
                //                                                   {
                //                                                       c.WhenStarted(s => s.Start());
                //                                                       c.WhenStopped(s => s.Stop());
                //                                                       c.WhenPaused(s => { });
                //                                                       c.WhenContinued(s => { });
                //                                                       c.CreateServiceLocator(()=>sl);
                //                                                   });
                

                x.DoNotStartAutomatically();

                x.RunAs("dru", "pass");

                //x.UseWinFormHost<MyForm>();

                x.DependsOn("ServiceName");
                x.DependencyOnMsmq();
                x.DependencyOnMsSql();
            });
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
            _runConfiguration.WinServiceSettings.FullDisplayName
                .ShouldEqual("chris");

            _runConfiguration.WinServiceSettings.FullServiceName
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
            _runConfiguration.Coordinator.Start();
            _runConfiguration.Coordinator.HostedServiceCount
                .ShouldEqual(1);

            IService service = _runConfiguration.Coordinator.GetService("my_service");

            service.Name
                .ShouldEqual("my_service");
            service.State
                .ShouldEqual(ServiceState.Started);
        }
    }
}