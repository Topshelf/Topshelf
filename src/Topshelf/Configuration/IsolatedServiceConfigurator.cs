namespace Topshelf.Configuration
{
    using System;
    using Internal;
    using Microsoft.Practices.ServiceLocation;

    public class IsolatedServiceConfigurator<TService> :
        IIsolatedServiceConfigurator<TService>
        where TService : MarshalByRefObject
    {
        private string _name = typeof(TService).Name;

        private Action<TService> _startAction = service => { };
        private Action<TService> _stopAction = service => { };
        private Action<TService> _pauseAction = service => { };
        private Action<TService> _continueAction = service => { };
        private Func<IServiceLocator> _createServiveLocator = () => ServiceLocator.Current;

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

        public void CreateServiceLocator(Func<IServiceLocator> fun)
        {
            _createServiveLocator = fun;
        }


        public IService Create()
        {
            IService service = new FacadeToIsolatedService<TService>(_createServiveLocator, _name, _startAction, _stopAction, _pauseAction, _continueAction);
            return service;
        }

        #region Dispose Crap
        private bool _disposed;
        

        ~IsolatedServiceConfigurator()
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