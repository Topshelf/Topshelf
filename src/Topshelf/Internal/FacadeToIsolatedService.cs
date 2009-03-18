namespace Topshelf.Internal
{
    using System;
    using Microsoft.Practices.ServiceLocation;

    [Serializable]
    public class FacadeToIsolatedService<TService> :
        IService
    {
        public Type ServiceType { get { return _remoteService.ServiceType; } }
        public ServiceState State { get
        {
            return _remoteService.State;
        } }
        public string Name { get { return _remoteService.Name; } }
        private AppDomain _domain;
        private IsolatedService<TService> _remoteService;

        //what to do with these
        Func<IServiceLocator> createServiceLocator;
        Action<TService> startAction;
        Action<TService> stopAction;
        Action<TService> pauseAction;
        Action<TService> continueAction;
        string _name; 

        public FacadeToIsolatedService(Func<IServiceLocator> sl, string name, Action<TService> startAction, Action<TService> stopAction, Action<TService> pauseAction, Action<TService> continueAction)
        {
            _name = name;
            this.createServiceLocator = sl;
            this.startAction = startAction;
            this.stopAction = stopAction;
            this.pauseAction = pauseAction;
            this.continueAction = continueAction;
        }

        public void Start()
        {
            var settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";
            _domain = AppDomain.CreateDomain(typeof (TService).AssemblyQualifiedName, null, settings);
            
            _remoteService = _domain.CreateInstanceAndUnwrap<IsolatedService<TService>>(createServiceLocator, _name, startAction, stopAction, pauseAction, continueAction);
            _remoteService.Start();
        }

        public void Stop()
        {
            _remoteService.IfNotNull(x=>x.Stop());
            AppDomain.Unload(_domain);
        }

        public void Pause()
        {
            _remoteService.IfNotNull(x => x.Pause());
        }

        public void Continue()
        {
            _remoteService.IfNotNull(x => x.Continue());
        }
    }

    public static class ObjectExtensions
    {
        public static void IfNotNull<TService>(this IsolatedService<TService> service, Action<IsolatedService<TService>> action)
        {
            if (service != null)
                action(service);
        }
    }
}