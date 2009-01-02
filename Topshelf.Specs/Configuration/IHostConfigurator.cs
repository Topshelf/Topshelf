namespace Topshelf.Specs.Configuration
{
    using System;
    using System.Windows.Forms;

    public interface IHostConfigurator : IDisposable
    {
        void ConfigureService<TService>();
        void ConfigureService<TService>(Action<IServiceConfigurator<TService>> action);

        void SetDisplayName(string displayName);
        void SetServiceName(string serviceName);
        void SetDescription(string description);

        /// <summary>
        /// We set the service to start automatically by default. This sets the service to manual instead.
        /// </summary>
        void DoNotStartAutomatically();

        void RunAsLocalSystem();
        void RunAs(string username, string password);
        void DependsOn(string serviceName);
        void DependencyOnMsmq();
        void DependencyOnMsSql();
        void RunAsFromInteractive();
        void UseWinFormRunner<T>() where T : Form;
    }
}