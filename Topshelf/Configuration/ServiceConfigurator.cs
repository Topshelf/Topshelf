namespace Topshelf.Configuration
{
    using System;

    public class ServiceConfigurator<TService> :
        IServiceConfigurator<TService>

    {
        private string _name = typeof (TService).Name;

        private Action<TService> _startAction = service => { };
        private Action<TService> _stopAction = service => { };
        private Action<TService> _pauseAction = service => { };
        private Action<TService> _continueAction = service => { };

        public void WhenStarted(Action<TService> startAction)
        {
            _startAction = startAction;
        }

        public void WhenStopped(Action<TService> stopAction)
        {
            _stopAction = stopAction;
        }

        public void WhenPaused(Action<TService> pauseAction)
        {
            _pauseAction = pauseAction;
        }

        public void WhenContinued(Action<TService> continueAction)
        {
            _continueAction = continueAction;
        }

        public void WithName(string name)
        {
            _name = name;
        }

        public IService Create()
        {
            Service<TService> service = new Service<TService>
                                            {
                                                Name = _name,
                                                StartAction = _startAction,
                                                StopAction = _stopAction,
                                                PauseAction = _pauseAction,
                                                ContinueAction = _continueAction
                                            };

            return service;
        }

        #region Dispose Crap
        private bool _disposed;
        ~ServiceConfigurator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {

            }
            _disposed = true;
        }
        #endregion
    }
}