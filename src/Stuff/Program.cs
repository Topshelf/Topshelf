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

	internal class Program
	{
		private static void Main(string[] args)
		{
			var cfg = RunnerConfigurator.New(x =>
				{
					x.BeforeStart(h =>
						{
							ObjectFactory.Initialize(i =>
								{
									i.ForConcreteType<TownCrier>();
									i.ForConcreteType<ServiceConsole>(); //bah why do I have to register this?
								});
							ServiceLocator.SetLocatorProvider(() => new StructureMapServiceLocator());
						});
					x.AfterStop(h => { Console.WriteLine("stopping"); });
					
					x.ConfigureService<TownCrier>(s =>
						{
							s.WhenStarted(tc => tc.Start());
							s.WhenStopped(tc => tc.Stop());
							s.WithName("tc");
						});

					x.RunAsLocalSystem();

					x.SetDescription("Sample Topshelf Host");
					x.SetDisplayName("Stuff");
					x.SetServiceName("stuff");
				});

			Runner.Host(cfg, args);
		}
	}

	public class TownCrier
	{
		private readonly Timer _timer;

		public TownCrier()
		{
			_timer = new Timer(1000) {AutoReset = true};
			_timer.Elapsed += (sender, eventArgs) => Console.WriteLine(DateTime.Now);
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

	public class StructureMapServiceLocator :
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
			foreach (object instance in ObjectFactory.GetAllInstances(serviceType))
			{
				yield return instance;
			}
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