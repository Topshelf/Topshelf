namespace StuffOnAShelf
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Timers;
    using log4net;
    using log4net.Config;
    using Topshelf.Configuration.Dsl;
    using Topshelf.Shelving;

    public class AShelvedClock :
        Bootstrapper<TheClock>
    {
        public void InitializeHostedService(IServiceConfigurator<TheClock> cfg)
        {
            cfg.HowToBuildService(n=> new TheClock());
            cfg.WhenStarted(s=>
            {
                XmlConfigurator.Configure(new FileInfo(".\\clock.log4net.config"));
                s.Start();
            });
            cfg.WhenStopped(s=>s.Stop());
        }
    }

    public class TheClock
    {
        readonly Timer _timer;
        readonly ILog _log = LogManager.GetLogger(typeof (TheClock));

        public TheClock()
        {
            _timer = new Timer(1000) {AutoReset = true};
            _timer.Elapsed += (sender, eventArgs) => _log.Info(DateTime.Now);
        }

        public void Start()
        {
            File.WriteAllText(@"C:\development\Topshelf\src\logs\clock\test.txt", ConfigurationManager.AppSettings["name"]);
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
        }
    }
}
