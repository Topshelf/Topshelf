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
namespace Topshelf.Model
{
    using System;
    using System.Diagnostics;
    using Exceptions;
    using Magnum.Channels;
    using Magnum.Fibers;
    using Magnum.StateMachine;
    using Messages;

    [DebuggerDisplay("Service({Name}) is {State}")]
    public class ServiceController<TService> :
        StateMachine<ServiceController<TService>>,
        IServiceController
        where TService : class
    {
        #region StateMachine

        static ServiceController()
        {
            Define(() =>
            {
                Initially(
                    When(OnStart)
                        .Then(sc => sc.Initialize())
                        .Then(sc => sc.StartAction(sc._instance))
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
                        .Then(sc => sc.StopAction(sc._instance))
                        .TransitionTo(Stopped)
                    );
            });
        }

        public static Event OnStart { get; set; }
        public static Event OnStop { get; set; }
        public static Event OnPause { get; set; }
        public static Event OnContinue { get; set; }

        public static State Initial { get; set; }
        public static State Started { get; set; }
        public static State Stopped { get; set; }
        public static State Paused { get; set; }
        public static State Completed { get; set; }

        #endregion

        #region Messaging Start
        //readonly UntypedChannelAdapter _myChannel;
        //readonly WcfUntypedChannelProxy _hostChannel;
        //subscribe
        //bind the messages to the appropriate action.
        #endregion

        public ServiceController()
        {
//            _myChannel = new UntypedChannelAdapter(new ThreadPoolFiber());
//            _myChannel.Subscribe(s =>
//            {
//                s.Consume<ReadyService>().Using(m => Initialize());
//                s.Consume<StopService>().Using(m => Stop());
//                s.Consume<StartService>().Using(m => Start());
//                s.Consume<PauseService>().Using(m => Pause());
//                s.Consume<ContinueService>().Using(m => Continue());
//            });

            //TODO: need an alternative name? botttle? 
            //_hostChannel.Send(new BottleReady());
        }

        TService _instance;
        public Action<TService> StartAction { get; set; }
        public Action<TService> StopAction { get; set; }
        public Action<TService> PauseAction { get; set; }
        public Action<TService> ContinueAction { get; set; }
        public ServiceBuilder BuildService { get; set; }

        #region Dispose Shit

        bool _disposed;

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
            BuildService = null;
            _disposed = true;
        }

        ~ServiceController()
        {
            Dispose(false);
        }

        #endregion

        #region IServiceController Members

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


        public string Name { get; set; }

        public Type ServiceType
        {
            get { return typeof(TService); }
        }

        public ServiceState State
        {
            get { return (ServiceState) Enum.Parse(typeof(ServiceState), CurrentState.Name, true); }
        }

        #endregion

        void Initialize()
        {
            //TODO: do I need to pull it out by name?
            _instance = (TService)BuildService(Name);
            if (_instance == null) throw new CouldntBuildServiceException(Name, typeof(TService));
        }
    }
}