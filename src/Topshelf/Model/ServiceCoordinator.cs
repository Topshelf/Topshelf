// Copyright 2007-2010 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using log4net;
    using Magnum.Channels;
    using Magnum.Concurrency;
    using Magnum.Extensions;
    using Magnum.Threading;
    using Messages;
    using Shelving;


    [DebuggerDisplay("Hosting {HostedServiceCount} Services")]
    public class ServiceCoordinator :
        IServiceCoordinator
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(ServiceCoordinator));
        readonly Action<IServiceCoordinator> _afterStop;
        readonly Action<IServiceCoordinator> _beforeStart;
        readonly Action<IServiceCoordinator> _beforeStartingServices;

        readonly ReaderWriterLockedObject<Queue<KeyValuePair<string, Exception>>> _exceptions =
            new ReaderWriterLockedObject<Queue<KeyValuePair<string, Exception>>>(new Queue<KeyValuePair<string, Exception>>());

        readonly WcfChannelHost _hostChannel;
        readonly ChannelAdapter _myChannel;
        readonly List<Func<IServiceController>> _serviceConfigurators;

        readonly IList<IServiceController> _services = new List<IServiceController>();

        public ServiceCoordinator(Action<IServiceCoordinator> beforeStartingServices,
                                  Action<IServiceCoordinator> beforeStart, Action<IServiceCoordinator> afterStop)
        {
            _beforeStartingServices = beforeStartingServices;
            _beforeStart = beforeStart;
            _afterStop = afterStop;
            _serviceConfigurators = new List<Func<IServiceController>>();
            _myChannel = new ChannelAdapter();
            _hostChannel = WellknownAddresses.GetHostHost(_myChannel);

            _myChannel.Connect(s =>
                {
                    s.AddConsumerOf<ShelfFault>().UsingConsumer(HandleServiceFault);
                    s.AddConsumerOf<ServiceStarted>().UsingConsumer(msg => ServiceStartedAction.Invoke(msg));
                });
        }

        public IList<IServiceController> Services
        {
            get
            {
                LoadNewServiceConfigurations();

                return _services;
            }
        }

        public void Start()
        {
            //TODO: With Shelving this feels like it needs to become before 'host' start
            _log.Debug("Calling BeforeStartingServices");
            _beforeStartingServices(this);
            _log.Info("BeforeStart complete");

            int unstartedCount = Services.Count(x => x.State != ServiceState.Started);
            bool completed;
            long startedServices = 0;

            using (var startedLatch = new ManualResetEvent(false))
            {
                var countDown = new CountdownLatch(unstartedCount, () => startedLatch.Set());

                Action<ServiceStarted> action = msg =>
                    {
                        countDown.CountDown();
                        Interlocked.Increment(ref startedServices);
                    };

                ServiceStartedAction += action;

                _log.Debug("Start is now starting any subordinate services");
                foreach (IServiceController serviceController in Services)
                {
                    _log.InfoFormat("Starting subordinate service '{0}'", serviceController.Name);
                    serviceController.ControllerChannel.Send(new StartService());
                }

                completed = startedLatch.WaitOne(30.Seconds());
                ServiceStartedAction -= action;
            }
            
            if (!completed && (HostedServiceCount == 1 || startedServices == 0))
            {
                var qCount = _exceptions.ReadLock(q => q.Count);
                Exception ex = null;

                if (qCount > 0)
                {
                    var kvp = _exceptions.WriteLock(s => s.Dequeue());
                    ex = kvp.Value;
                }

                throw new Exception("One or more services failed to start in a timely manner.", ex);
            }

            int serviceExCount = _exceptions.ReadLock(q => q.Select(x => x.Key).Distinct().Count());
            if (serviceExCount >= HostedServiceCount)
            {
                throw new Exception("All services have errored out.", _exceptions.ReadLock(q => q.Dequeue()).Value);
            }

            //TODO: This feels like it should be after 'host' stop
            _log.Debug("Calling BeforeStart");
            _beforeStart(this);
            _log.Info("BeforeStart complete");
        }

        public void Stop()
        {
            //TODO: PRE STOP

            foreach (IServiceController service in Services.Reverse())
            {
                try
                {
                    _log.InfoFormat("Stopping sub service '{0}'", service.Name);
                    service.Stop();
                }
                catch (Exception ex)
                {
                    _log.Error("Exception stopping sub service " + service.Name, ex);
                }
            }
            //TODO: Need to wait for shut down
            _log.Debug("pre after stop");
            _afterStop(this);
            _log.Info("AfterStop complete");
        }

        public void Pause()
        {
            foreach (IServiceController service in Services)
            {
                _log.InfoFormat("Pausing sub service '{0}'", service.Name);
                service.Pause();
            }
        }

        public void Continue()
        {
            foreach (IServiceController service in Services)
            {
                _log.InfoFormat("Continuing sub service '{0}'", service.Name);
                service.Continue();
            }
        }

        public void StartService(string name)
        {
            if (Services.Count == 0)
                CreateServices();

            Services.Where(x => x.Name == name).First().ControllerChannel.Send(new StartService());
            //need a way to pause here
        }

        public void StopService(string name)
        {
            if (Services.Count == 0)
                CreateServices();

            Services.Where(x => x.Name == name).First().Stop();
        }

        public void PauseService(string name)
        {
            if (Services.Count == 0)
                CreateServices();

            Services.Where(x => x.Name == name).First().Pause();
        }

        public void ContinueService(string name)
        {
            if (Services.Count == 0)
                CreateServices();

            Services.Where(x => x.Name == name).First().Continue();
        }

        public int HostedServiceCount
        {
            get { return Services.Count; }
        }

        public IList<ServiceInformation> GetServiceInfo()
        {
            LoadNewServiceConfigurations();
            var result = new List<ServiceInformation>();

            foreach (IServiceController serviceController in Services)
            {
                result.Add(new ServiceInformation
                    {
                        Name = serviceController.Name,
                        State = serviceController.State,
                        Type = serviceController.ServiceType.Name
                    });
            }

            return result;
        }

        public IServiceController GetService(string name)
        {
            return Services.Where(x => x.Name == name).FirstOrDefault();
        }

        #region Dispose Crap

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                Services.Each(s => s.Dispose());
                Services.Clear();

                if (_hostChannel != null)
                    _hostChannel.Dispose();
            }
            _disposed = true;
        }

        ~ServiceCoordinator()
        {
            Dispose(false);
        }

        #endregion

        event Action<ServiceStarted> ServiceStartedAction;

        void LoadNewServiceConfigurations()
        {
            if (_serviceConfigurators.Any())
            {
                foreach (var serviceConfigurator in _serviceConfigurators)
                {
                    IServiceController serviceController = serviceConfigurator();
                    _services.Add(serviceController);
                }

                _serviceConfigurators.Clear();
            }
        }

        public void AddNewService(IServiceController controller)
        {
            _services.Add(controller);
            //TODO: How to best call start here?
        }

        public void RegisterServices(IList<Func<IServiceController>> services)
        {
            _serviceConfigurators.AddRange(services);
        }

        void CreateServices()
        {
            foreach (var serviceConfigurator in _serviceConfigurators)
            {
                IServiceController serviceController = serviceConfigurator();
                Services.Add(serviceController);
            }
        }

        public event Action<KeyValuePair<string, Exception>> ShelfFaulted;

        void HandleServiceFault(ShelfFault faultMessage)
        {
            var fault = new KeyValuePair<string, Exception>(faultMessage.ShelfName, faultMessage.Exception);

            _exceptions.WriteLock(s => s.Enqueue(fault));

            Action<KeyValuePair<string, Exception>> handle = ShelfFaulted;
            if (handle != null)
                handle.Invoke(fault);
        }
    }
}