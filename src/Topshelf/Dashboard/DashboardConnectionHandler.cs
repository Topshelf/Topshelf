namespace Topshelf.Dashboard
{
    using Model;
    using Stact;
    using Stact.ServerFramework;


    public class DashboardConnectionHandler :
        PatternMatchConnectionHandler
    {
        readonly StatusChannel _statusChannel;

        public DashboardConnectionHandler(ServiceCoordinator serviceCoordinator) :
            base("^/dashboard", "GET")
        {
            _statusChannel = new StatusChannel(serviceCoordinator);
        }

        protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
        {
            return _statusChannel;
        }


        class StatusChannel :
            Channel<ConnectionContext>
        {
            readonly ServiceCoordinator _serviceCoordinator;
            readonly Fiber _fiber;

            public StatusChannel(ServiceCoordinator serviceCoordinator)
            {
                _serviceCoordinator = serviceCoordinator;
                _fiber = new PoolFiber();
            }

            public void Send(ConnectionContext context)
            {
                _fiber.Add(() =>
                    {

                        var infos = _serviceCoordinator.Status();
                        var view = new DashboardView(infos);

                        context.Response.RenderSparkView(view, "dashboard.html");
                        context.Complete();
                    });
            }
        }
    }
}