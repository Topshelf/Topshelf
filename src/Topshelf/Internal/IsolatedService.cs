namespace Topshelf.Internal
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    public class IsolatedService<TService> :
        MarshalByRefObject,
        IService
    {
        public IsolatedService(Func<IServiceLocator> serviceLocator, string name, Action<TService> startAction, Action<TService> stopAction, Action<TService> pauseAction, Action<TService> continueAction)
        {
            this.ServiceLocator = serviceLocator();
            this.Name = name;
            this.StartAction = startAction;
            this.StopAction = stopAction;
            this.PauseAction = pauseAction;
            this.ContinueAction = continueAction;
            State = ServiceState.Stopped;
        }

        public Type ServiceType
        {
            get
            {
                return typeof(TService);
            }
        }

        public IServiceLocator ServiceLocator { get; set; }
        public ServiceState State { get; private set; }
        public string Name { get; private set; }
        public Action<TService> StartAction { get; private set; }
        public Action<TService> StopAction { get; private set; }
        public Action<TService> PauseAction { get; private set; }
        public Action<TService> ContinueAction { get; private set; }
        private TService _instance;

        public void Start()
        {
            _instance = ServiceLocator.GetInstance<TService>(Name);
            StartAction(_instance);
            State = ServiceState.Started;
        }

        public void Stop()
        {
            StopAction(_instance);
            State = ServiceState.Stopped;
        }

        public void Pause()
        {
            PauseAction(_instance);
            State = ServiceState.Paused;
        }

        public void Continue()
        {
            ContinueAction(_instance);
            State = ServiceState.Started;
        }
    }
}