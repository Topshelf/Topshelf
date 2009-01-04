namespace Stuff
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Timers;
    using Microsoft.Practices.ServiceLocation;
    using StructureMap;
    using Topshelf;
    using Topshelf.Configuration;

    class Program
    {
        static void Main(string[] args)
        {

            var host = HostConfigurator.New(x=>
                                                {
                                                    x.BeforeStart(h =>
                                                      {
                                                          ObjectFactory.Initialize(i=>
                                                           {
                                                               i.ForConcreteType<TownCrier>();
                                                           });
                                                          ServiceLocator.SetLocatorProvider(()=>new  StructureMapObjectBuilder());
                                                      });
                                                    x.ConfigureService<TownCrier>(s=>
                                                                                      {
                                                                                          s.WhenStarted(tc => tc.Start());
                                                                                          s.WhenStopped(tc=>tc.Stop());
                                                                                          s.WithName("tc");
                                                                                      }
                                                        );

                                                    x.RunAsLocalSystem();
                                                    
                                                    x.SetDescription("Sample Topshelf Host");
                                                    x.SetDisplayName("Stuff");
                                                    x.SetServiceName("stuff");
                                                });

            Runner.Run(host, args);
        }
    }

    public class TownCrier
    {
        private Timer _timer;

        public TownCrier()
        {
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += Bob;
        }

        private void Bob(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(DateTime.Now);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }

    public class StructureMapObjectBuilder :
        IServiceLocator
    {
        public object GetService(Type serviceType)
        {
            return ObjectFactory.GetInstance(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            return ObjectFactory.GetInstance(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            return ObjectFactory.GetNamedInstance(serviceType, key);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var result = new ArrayList(ObjectFactory.GetAllInstances(serviceType));
            return result.ToArray();
        }

        public TService GetInstance<TService>()
        {
            return ObjectFactory.GetInstance<TService>();
        }

        public TService GetInstance<TService>(string key)
        {
            return ObjectFactory.GetNamedInstance<TService>(key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return ObjectFactory.GetAllInstances<TService>();
        }
    }
}
