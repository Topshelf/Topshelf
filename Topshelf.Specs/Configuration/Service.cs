namespace Topshelf.Specs.Configuration
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    public class Service<TService> :
        IService
    {
        public Type ServiceType
        {
            get
            {
                return typeof(TService);
            }
        }

        public string Name { get; set; }
        public Action<TService> StartAction{ get; set;}
        public Action<TService> StopAction{ get; set;}
        public Action<TService> PauseAction{ get; set;}
        public Action<TService> ContinueAction{ get; set;}

        public void Start()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            StartAction(instance);
        }

        public void Stop()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            StopAction(instance);
        }

        public void Pause()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            PauseAction(instance);
        }

        public void Continue()
        {
            TService instance = ServiceLocator.Current.GetInstance<TService>();
            ContinueAction(instance);
        }
    }
}