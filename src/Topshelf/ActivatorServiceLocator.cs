using Microsoft.Practices.ServiceLocation;

namespace Topshelf
{
    using System;
    using System.Collections.Generic;

    public class ActivatorServiceLocator :
        IServiceLocator
    {
        public object GetService(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return Activator.CreateInstance(serviceType);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return new []{Activator.CreateInstance(serviceType)};
        }

        public TService GetInstance<TService>()
        {
            return (TService)Activator.CreateInstance(typeof(TService));
        }

        public TService GetInstance<TService>(string key)
        {
            return (TService)Activator.CreateInstance(typeof(TService));
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return new[] { (TService)Activator.CreateInstance(typeof(TService)) };
        }
    }
}
