namespace Topshelf.Hosts
{
    using Logging;
	using Windows;


	public class StopHost :
		Host
	{
		readonly ILog _log = Logger.Get("Topshelf.Hosts.StopHost");

		public StopHost(ServiceDescription description)
		{
			Description = description;
		}

		public ServiceDescription Description { get; private set; }

		public void Run()
		{
			if (!WindowsServiceControlManager.IsInstalled(Description.GetServiceName()))
			{
				string message = string.Format("The {0} service is not installed.", Description.GetServiceName());
				_log.Error(message);

				return;
			}

			if (!WindowsUserAccessControl.IsAdministrator)
			{
				if (!WindowsUserAccessControl.RerunAsAdministrator())
					_log.ErrorFormat("The {0} service can only be stopped by an administrator", Description.GetServiceName());

				return;
			}

			_log.DebugFormat("Attempting to stop '{0}'", Description.GetServiceName());

			WindowsServiceControlManager.Stop(Description.GetServiceName());

			_log.InfoFormat("The {0} service has been stopped.", Description.GetServiceName());
		}
	}
}