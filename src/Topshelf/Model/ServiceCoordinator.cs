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

        readonly ReaderWriterLockedObject<Queue<Exception>> _exceptions =
            new ReaderWriterLockedObject<Queue<Exception>>(new Queue<Exception>());

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
                    s.AddConsumerOf<ServiceStopped>().UsingConsumer(msg => ServiceStoppedAction.Invoke(msg));
                    s.AddConsumerOf<ServiceContinued>().UsingConsumer(msg => ServiceContinuedAction.Invoke(msg));
                    s.AddConsumerOf<ServicePaused>().UsingConsumer(msg => ServicePausedAction.Invoke(msg));
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

        void ProcessEvent<TSent, TRecieved>(string printableMethod, string printableAction, ref Action<TRecieved> stateEvent, ServiceState targetState)
            where TRecieved : ServiceMessage
            where TSent : ServiceMessage
        {
            int servicesNotInTargetState = Services.Count(x => x.State != targetState);
            bool completed;
            long serviceReachedTargetState = 0;

            using (var startedLatch = new ManualResetEvent(false))
            {
                var countDown = new CountdownLatch(servicesNotInTargetState, () => startedLatch.Set());

                Action<TRecieved> action = msg =>
                {
                    countDown.CountDown();
                    Interlocked.Increment(ref serviceReachedTargetState);
                };

                stateEvent += action;

                _log.Debug("{0} is now {1} any subordinate services".FormatWith(printableMethod, printableAction));
                foreach (IServiceController serviceController in Services)
                {
                    _log.InfoFormat("{1} subordinate service '{0}'", serviceController.Name, printableAction);
                    serviceController.ControllerChannel.Send(default(TSent));
                }

                completed = startedLatch.WaitOne(30.Seconds());
                stateEvent -= action;
            }

            if (!completed && (HostedServiceCount == 1 || serviceReachedTargetState == 0))
            {
                int qCount = _exceptions.ReadLock(q => q.Count);
                Exception ex = null;

                if (qCount > 0)
                    ex = _exceptions.WriteLock(s => s.Dequeue());

                throw new Exception("One or more services failed to {0} in a timely manner.".FormatWith(printableMethod), ex);
            }

            if (!Services.Any(x => x.State == targetState))
                throw new Exception("All services have errored out.", _exceptions.ReadLock(q => q.Dequeue()));
        }

        public void Start()
        {
            //TODO: With Shelving this feels like it needs to become before 'host' start
            _log.Debug("Calling BeforeStartingServices");
            _beforeStartingServices(this);
            _log.Info("BeforeStart complete");

            ProcessEvent<StartService, ServiceStarted>("Start", "starting", ref ServiceStartedAction, ServiceState.Started);

            //TODO: This feels like it should be after 'host' stop
            _log.Debug("Calling BeforeStart");
            _beforeStart(this);
            _log.Info("BeforeStart complete");
        }

        public void Stop()
        {
            //TODO: PRE STOP

            ProcessEvent<StopService, ServiceStopped>("Stop", "stopping", ref ServiceStoppedAction, ServiceState.Stopped);

            //TODO: Need to wait for shut down
            _log.Debug("pre after stop");
            _afterStop(this);
            _log.Info("AfterStop complete");
        }

        public void Pause()
        {
            ProcessEvent<PauseService, ServicePaused>("Pause", "pausing", ref ServicePausedAction, ServiceState.Paused);
        }

        public void Continue()
        {
            ProcessEvent<ContinueService, ServiceContinued>("Continue", "continuing", ref ServiceContinuedAction, ServiceState.Started);
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

            return Services
                .ToList()
                .ConvertAll(serviceController => new ServiceInformation
                    {
                        Name = serviceController.Name,
                        State = serviceController.State,
                        Type = serviceController.ServiceType.Name
                    });
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
        event Action<ServiceStopped> ServiceStoppedAction;
        event Action<ServicePaused> ServicePausedAction;
        event Action<ServiceContinued> ServiceContinuedAction;
        public event Action<Exception> ShelfFaulted;

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

        void HandleServiceFault(ShelfFault faultMessage)
        {
            _exceptions.WriteLock(s => s.Enqueue(faultMessage.Exception));

            Action<Exception> handle = ShelfFaulted;
            if (handle != null)
                handle.Invoke(faultMessage.Exception);
        }
    }
}