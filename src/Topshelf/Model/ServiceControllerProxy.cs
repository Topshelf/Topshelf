namespace Topshelf.Internal
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    public class ServiceControllerProxy : 
        MarshalByRefObject,
        IServiceController
    {
        readonly IServiceControllerOf<object> _target;

        public ServiceControllerProxy(Type type)
        {
            var targetType = typeof(IsolatedServiceControllerWrapper<>).MakeGenericType(type);
            _target = (IServiceControllerOf<object>)Activator.CreateInstance(targetType);
        }

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

        public Action<object> StartAction
        {
            get { return _target.StartAction; }
            set { _target.StartAction = value; }
        }

        public Action<object> StopAction
        {
            get { return _target.StopAction; }
            set { _target.StopAction = value; }
        }

        public Action<object> PauseAction
        {
            get { return _target.PauseAction; }
            set { _target.PauseAction = value; }
        }

        public Action<object> ContinueAction
        {
            get { return _target.ContinueAction; }
            set { _target.ContinueAction = value; }
        }

        public Func<IServiceLocator> CreateServiceLocator
        {
            get { return _target.CreateServiceLocator; }
            set { _target.CreateServiceLocator = value; }
        }
    }
}