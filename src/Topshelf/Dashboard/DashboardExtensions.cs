namespace Topshelf
{
	using Dashboard;
	using HostConfigurators;


	public static class DashboardExtensions
	{
		public static void EnableDashboard(this HostConfigurator configurator)
		{
			configurator.Service<TopshelfDashboard>(o =>
			{
				o.ConstructUsing((description, name, coordinator) => new TopshelfDashboard(description, coordinator));
				o.WhenStarted(s => s.Start());
				o.WhenStopped(s => s.Stop());
			});
		}

	}
}