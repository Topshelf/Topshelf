namespace Topshelf.Internal
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    [Serializable]
    public class ControllerDelegates<TService> where TService : class
    {
        public Action<TService> StartAction { get; set; }
        public Action<TService> StopAction { get; set; }
        public Action<TService> PauseAction { get; set; }
        public Action<TService> ContinueAction { get; set; }
        public Func<IServiceLocator> CreateServiceLocator { get; set; }

        public void StartActionObject(object obj)
        {
            StartAction((TService)obj);
        }

        public void StopActionObject(object obj)
        {
            StopAction((TService)obj);
        }

        public void PauseActionObject(object obj)
        {
            PauseAction((TService)obj);
        }

        public void ContinueActionObject(object obj)
        {
            ContinueAction((TService)obj);
        }
    }
}