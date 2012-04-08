namespace Topshelf.NancyDashboard
{
    using System;
    using Nancy.Hosting.Self;
    using Topshelf;
    using Topshelf.Logging;
    using Topshelf.Model;


    public class TopshelfDashboard
    {
        readonly ILog _log = Logger.Get("Topshelf.WebControl.WebControl");

        readonly ServiceDescription _description;
        readonly IServiceChannel _serviceCoordinator;
        NancyHost _nancyHost;

        public Uri ServerUri { get; set; }

        public TopshelfDashboard(ServiceDescription description, IServiceChannel serviceCoordinator)
        {
            _description = description;
            _serviceCoordinator = serviceCoordinator;
            TinyIoC.TinyIoCContainer.Current.Register<IServiceChannel>(_serviceCoordinator);
        }

        public void Start()
        {
            ServerUri = new Uri("http://localhost:8085");
            _log.InfoFormat("Loading dashboard at Uri: {0}", ServerUri);

            _nancyHost = new NancyHost(ServerUri);
            _nancyHost.Start();
        }

        public void Stop()
        {
            _nancyHost.Stop();
            _nancyHost = null;
        }
    }
}