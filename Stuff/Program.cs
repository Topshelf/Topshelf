namespace Stuff
{
    using System;
    using System.Timers;
    using Topshelf;
    using Topshelf.Configuration;

    class Program
    {
        static void Main(string[] args)
        {
            var host = HostConfigurator.New(x=>
                                                {
                                                    x.ConfigureService<TownCrier>(s=>
                                                                                      {
                                                                                          s.WhenStarted(tc => tc.Start());
                                                                                          s.WhenStopped(tc=>tc.Stop());
                                                                                          s.WithName("tc");
                                                                                      }
                                                        );

                                                    x.RunAsLocalSystem();
                                                    
                                                    x.SetDescription("Sample Topshelf Host");
                                                    x.SetDisplayName("Stuff");
                                                    x.SetServiceName("stuff");
                                                });

            Runner.Run(host, args);
        }
    }

    public class TownCrier
    {
        private Timer _timer;

        public TownCrier()
        {
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += Bob;
        }

        private void Bob(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(DateTime.Now);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
