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
    using log4net;
    using Magnum.Channels;
    using Messages;
    using Magnum.Extensions;
    using Shelving;


    [DebuggerDisplay("Hosting {HostedServiceCount} Services")]
    public class ServiceCoordinator :
        IServiceCoordinator
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(ServiceCoordinator));
        readonly Action<IServiceCoordinator> _afterStop;
        readonly Action<IServiceCoordinator> _beforeStart;
        readonly Action<IServiceCoordinator> _beforeStartingServices;
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

            _myChannel.Connect(s => { });
        }

        public IList<IServiceController> Services
        {
            get
            {
                LoadNewServiceConfigurations();

                return _services;
            }
        }

        void LoadNewServiceConfigurations()
        {
            if (_serviceConfigurators.Any())
            {
                foreach (Func<IServiceController> serviceConfigurator in _serviceConfigurators)
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

        public void Start()
        {
            //TODO: With Shelving this feels like it needs to become before 'host' start
            _log.Debug("Calling BeforeStartingServices");
            _beforeStartingServices(this);
            _log.Info("BeforeStart complete");

            _log.Debug("Start is now starting any subordinate services");

            foreach (var serviceController in Services)
            {
                _log.InfoFormat("Starting subordinate service '{0}'", serviceController.Name);
                serviceController.ControllerChannel.Send(new StartService());
            }

            //TODO: This feels like it should be after 'host' stop
            _log.Debug("Calling BeforeStart");
            _beforeStart(this);
            _log.Info("BeforeStart complete");
        }

        public void Stop()
        {
            //TODO: PRE STOP

            foreach (var service in Services.Reverse())
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
            foreach (var service in Services)
            {
                _log.InfoFormat("Pausing sub service '{0}'", service.Name);
                service.Pause();
            }
        }

        public void Continue()
        {
            foreach (var service in Services)
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

            foreach (var serviceController in Services)
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

        readonly List<Func<IServiceController>> _serviceConfigurators;
        bool _disposed;
        ChannelAdapter _myChannel;
        WcfChannelHost _hostChannel;

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

        public void RegisterServices(IList<Func<IServiceController>> services)
        {
            _serviceConfigurators.AddRange(services);
        }

        void CreateServices()
        {
            foreach (Func<IServiceController> serviceConfigurator in _serviceConfigurators)
            {
                IServiceController serviceController = serviceConfigurator();
                Services.Add(serviceController);
            }
        }
    }
}