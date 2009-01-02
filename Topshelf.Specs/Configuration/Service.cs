namespace Topshelf.Specs.Configuration
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    public class Service<TService> :
        IService
    {
        public Service()
        {
            State = ServiceState.Stopped;
        }

        public Type ServiceType
        {
            get
            {
                return typeof(TService);
            }
        }

        public ServiceState State { get; private set; }
        public string Name { get; set; }
        public Action<TService> StartAction{ get; set;}
        public Action<TService> StopAction{ get; set;}
        public Action<TService> PauseAction{ get; set;}
        public Action<TService> ContinueAction{ get; set;}

        public void Start()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            StartAction(instance);
            State = ServiceState.Started;
        }

        public void Stop()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            StopAction(instance);
            State = ServiceState.Stopped;
        }

        public void Pause()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            PauseAction(instance);
            State = ServiceState.Paused;
        }

        public void Continue()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            ContinueAction(instance);
            State = ServiceState.Started;
        }
    }
}