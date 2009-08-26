// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf.Internal
{
    using System;
    using System.Diagnostics;
    using Exceptions;
    using Magnum.StateMachine;
    using Microsoft.Practices.ServiceLocation;

    [DebuggerDisplay("Service({Name}) is {State}")]
    public class ServiceController<TService> : 
        StateMachine<ServiceController<TService>>,
        IServiceController
        where TService : class
    {
        #region StateMachine
        static ServiceController()
        {
            Define(()=>
            {
                Initially(
                    When(OnStart)
                        .Then(sc=>sc.BuildInstance() )
                            .Then(sc=>sc.StartAction(sc._instance))
                            .TransitionTo(Started)
                        );

                During(Started,
                       When(OnPause)
                           .Then(sc => sc.PauseAction(sc._instance))
                           .TransitionTo(Paused));

                During(Paused,
                       When(OnContinue)
                           .Then(sc => sc.ContinueAction(sc._instance))
                           .TransitionTo(Started));

                
                Anytime(
                    When(OnStop)
                        .Then(sc=> sc.StopAction(sc._instance))
                        .TransitionTo(Stopped)
                    );
            });
        }

        public static Event OnStart{ get; set;}
        public static Event OnStop { get; set; }
        public static Event OnPause { get; set; }
        public static Event OnContinue { get; set; }

        public static State Initial { get; set; }
        public static State Started { get; set; }
        public static State Stopped { get; set; }
        public static State Paused { get; set; }
        public static State Completed { get; set; }
        #endregion
        
        private TService _instance;
        private IServiceLocator _serviceLocator;


        public void Start()
		{
		    RaiseEvent(OnStart);
		}
		public void Stop()
		{
            RaiseEvent(OnStop);
		}
		public void Pause()
		{
		    RaiseEvent(OnPause);
		}
		public void Continue()
		{
		    RaiseEvent(OnContinue);
		}
        private void BuildInstance()
        {
            _instance = ServiceLocator.GetInstance<TService>(Name);
            if (_instance == null) throw new CouldntFindServiceException(Name, typeof(TService));
        }


        public string Name { get; set; }
        public Type ServiceType
        {
            get { return typeof(TService); }
        }
        public IServiceLocator ServiceLocator
        {
            get
            {
                if (_serviceLocator == null)
                {
                    _serviceLocator = CreateServiceLocator();
                }

                return _serviceLocator;
            }
        }
        public ServiceState State
        {
            get
            {
                return (ServiceState)Enum.Parse(typeof(ServiceState), CurrentState.Name, true);
            }
        }

        public Action<TService> StartAction { get; set; }
        public Action<TService> StopAction { get; set; }
        public Action<TService> PauseAction { get; set; }
        public Action<TService> ContinueAction { get; set; }
        public Func<IServiceLocator> CreateServiceLocator { get; set; }

        

        #region Dispose Shit
        private bool _disposed;
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (!disposing) return;
            if (!_disposed) return;

            
			_instance = default(TService);
            StartAction = null;
            StopAction = null;
            PauseAction = null;
            ContinueAction = null;
            CreateServiceLocator = null;
            _serviceLocator = null;
            _disposed = true;
		}

		~ServiceController()
		{
			Dispose(false);
		}
        #endregion
    }
}