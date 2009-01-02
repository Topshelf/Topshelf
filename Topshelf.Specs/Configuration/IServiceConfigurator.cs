namespace Topshelf.Specs.Configuration
{
    using System;

    public interface IServiceConfigurator<TService> :
        IDisposable
    {
        void WhenStarted(Action<TService> startAction);
        void WhenStopped(Action<TService> stopAction);
        void WhenPaused(Action<TService> pauseAction);
        void WhenContinued(Action<TService> continueAction);
        void WithName(string name);
    }
}