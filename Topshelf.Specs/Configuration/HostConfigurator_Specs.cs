namespace Topshelf.Specs.Configuration
{
    using MbUnit.Framework;

    [TestFixture]
    public class HostConfigurator_Specs
    {
        [Test]
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
    }

    public class TestService
    {
        public void Start()
        {
        }

        public void Stop()
        {
            
        }
    }

    //public class MyForm : 
    //    Form
    //{
    //}
}