namespace Topshelf.Specs.Configuration
{
    using System;
    using System.Collections.Generic;
    using Actions;

    public class Host :
        IHost
    {
        private readonly IDictionary<string, IService> _services = new Dictionary<string, IService>();
        private Type _formType;
        private NamedAction _action;

        public WinServiceSettings WinServiceSettings { get; set; }
        public Credentials Credentials { get; set; }

        public void Start()
        {
            foreach (var service in _services.Values)
            {
                service.Start();
            }
        }

        public void Stop()
        {
            foreach (var service in _services.Values)
            {
                service.Stop();
            }
        }

        public void Pause()
        {
            foreach (var service in _services.Values)
            {
                service.Pause();
            }
        }

        public void Continue()
        {
            foreach (var service in _services.Values)
            {
                service.Continue();
            }
        }

        public void StartService(string name)
        {
            _services[name].Start();
        }

        public void StopService(string name)
        {
            _services[name].Stop();
        }

        public void PauseService(string name)
        {
            _services[name].Pause();
        }

        public void ContinueService(string name)
        {
            _services[name].Continue();
        }

        public void RegisterServices(IList<IService> services)
        {
            foreach (var service in services)
            {
                _services.Add(service.Name, service);
            }
        }

        public void SetRunnerAction(NamedAction action, Type form)
        {
            _action = action;
            _formType = form;
        }
    }
}