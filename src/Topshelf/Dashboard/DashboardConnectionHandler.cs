namespace Topshelf.Dashboard
{
    using Stact;
    using Stact.ServerFramework;


    public class DashboardConnectionHandler :
        PatternMatchConnectionHandler
    {
        readonly StatusChannel _statusChannel;

        public DashboardConnectionHandler() :
            base("^/dashboard", "GET")
        {
            _statusChannel = new StatusChannel();
        }

        protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
        {
            return _statusChannel;
        }


        class StatusChannel :
            Channel<ConnectionContext>
        {
            readonly Fiber _fiber;

            public StatusChannel( )
            {
                _fiber = new PoolFiber();
            }

            public void Send(ConnectionContext context)
            {
                _fiber.Add(() =>
                {
                    context.Response.RenderSparkView(new DashboardView(), "dashboard.html");
                    context.Complete();
                });
            }
        }
    }

    public class DashboardView
    {
        public DashboardView()
        {
            Name = "dru";
        }

        public string Name { get; set; }
    }
}