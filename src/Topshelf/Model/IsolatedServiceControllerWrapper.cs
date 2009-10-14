namespace Topshelf.Internal
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    public class IsolatedServiceControllerWrapper<TService> :
        IServiceControllerOf<object> where TService : class
    {
        readonly ServiceController<TService> _target = new ServiceController<TService>();

        public void Dispose()
        {
            _target.Dispose();
        }

        public Type ServiceType
        {
            get { return _target.ServiceType; }
        }

        public string Name
        {
            get { return _target.Name; }
            set { _target.Name = value; }
        }

        public ServiceState State
        {
            get { return _target.State; }
        }

        public IServiceLocator ServiceLocator
        {
            get { return _target.ServiceLocator; }
        }

        public void Start()
        {
            _target.Start();
        }

        public void Stop()
        {
            _target.Stop();
        }

        public void Pause()
        {
            _target.Pause();
        }

        public void Continue()
        {
            _target.Continue();
        }

        public void ChangeName(string value)
        {
            _target.Name = value;
        }

        public Action<object> StartAction
        {
            get { return service => _target.StartAction((TService)service); }
            set { _target.StartAction = service => value(service); }
        }

        public Action<object> StopAction
        {
            get { return service => _target.StopAction((TService)service); }
            set { _target.StopAction = service => value(service); }
        }

        public Action<object> PauseAction
        {
            get { return service => _target.PauseAction((TService)service); }
            set { _target.PauseAction = service => value(service); }
        }

        public Action<object> ContinueAction
        {
            get { return service => _target.ContinueAction((TService)service); }
            set { _target.ContinueAction = service => value(service); }
        }

        public Func<IServiceLocator> CreateServiceLocator
        {
            get { return _target.CreateServiceLocator; }
            set { _target.CreateServiceLocator = value; }
        }
    }
}