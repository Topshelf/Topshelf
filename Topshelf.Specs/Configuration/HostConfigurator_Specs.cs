namespace Topshelf.Specs.Configuration
{
    using System.ServiceProcess;
    using MbUnit.Framework;

    [TestFixture]
    public class HostConfigurator_Specs
    {
        private Host _host;
        [SetUp]
        public void EstablishContext()
        {
            _host = (Host)HostConfigurator.New(x =>
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

                //x.UseWinFormRunner<MyForm>();

                x.DependsOn("ServiceName");
                x.DependencyOnMsmq();
                x.DependencyOnMsSql();
            });
        }
        //[Test]
        public void Syntax_Play()
        {
            IHost host = HostConfigurator.New(x =>
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

                          //x.UseWinFormRunner<MyForm>();

                          x.DependsOn("ServiceName");
                          x.DependencyOnMsmq();
                          x.DependencyOnMsSql();
                      });


            //host.TakeAction(args);
        }

        [Test]
        public void A_pretend_void_main()
        {
            string[] args = new string[0];
            IHost host = HostConfigurator.New(x => { });
            //some thing parses the args
            //Dispatch(args, host);
        }

        [Test]
        public void Should_depend_on_Msmq_MsSql_and_Custom()
        {
            _host.WinServiceSettings.Dependencies
                .ShouldContain(KnownServiceNames.Msmq);

            _host.WinServiceSettings.Dependencies
                .ShouldContain(KnownServiceNames.SqlServer);

            _host.WinServiceSettings.Dependencies
                .ShouldContain("ServiceName");
        }

        [Test]
        public void Names_should_be_correct()
        {
            _host.WinServiceSettings.DisplayName
                .ShouldEqual("chris");

            _host.WinServiceSettings.ServiceName
                .ShouldEqual("chris");

            _host.WinServiceSettings.Description
                .ShouldEqual("chris's pants");
        }

        [Test]
        public void Should_not_be_set_to_start_automatically()
        {
            _host.WinServiceSettings.StartMode
                .ShouldEqual(ServiceStartMode.Manual);
        }

        [Test]
        public void Credentials()
        {
            _host.Credentials.Username
                .ShouldEqual("dru");

            _host.Credentials.Password
                .ShouldEqual("pass");

            _host.Credentials.AccountType
                .ShouldEqual(ServiceAccount.User);
        }

        [Test]
        public void Hosted_service_configuration()
        {
            _host.HostedServiceCount
                .ShouldEqual(2);

            IService service = _host.GetService("my_service");

            service.Name
                .ShouldEqual("my_service");
            service.State
                .ShouldEqual(ServiceState.Stopped);
        }

    }

    public class TestService
    {
        public bool Started;
        public bool Stopped;

        public void Start()
        {
            Started = true;
            Stopped = false;
        }

        public void Stop()
        {
            Started = false;
            Stopped = true;
        }
    }

    //public class MyForm : 
    //    Form
    //{
    //}
}