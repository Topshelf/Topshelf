﻿// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Topshelf.Shelving
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Text;
    using System.Threading;
    using log4net;
    using Magnum.Channels;
    using Magnum.Concurrency;
    using Magnum.Extensions;
    using Magnum.StateMachine;
    using Magnum.Threading;
    using Messages;
    using Event = Magnum.StateMachine.Event;

    public class ShelfMaker :
        IDisposable
    {
        readonly HostHost _myChannelHost;
        readonly ChannelAdapter _myChannel;
        readonly ReaderWriterLockedObject<Dictionary<string, ShelfHandle>> _shelves;
        static readonly ILog _log = LogManager.GetLogger(typeof(ShelfMaker));

        public ShelfMaker()
        {
            _shelves = new ReaderWriterLockedObject<Dictionary<string, ShelfHandle>>(new Dictionary<string, ShelfHandle>());

            _myChannel = new ChannelAdapter();
            _myChannelHost = WellknownAddresses.GetShelfMakerHost(_myChannel);

            _myChannel.Connect(s =>
            {
                s.AddConsumerOf<ShelfReady>().UsingConsumer(MarkShelfReadyAndInitService);
				s.AddConsumerOf<ServiceReady>().UsingConsumer(MarkServiceReadyAndStart);
				s.AddConsumerOf<ServiceStopped>().UsingConsumer(MarkShelvedServiceStopped);
				s.AddConsumerOf<ServiceStarted>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Started));
				s.AddConsumerOf<FileSystemChange>().UsingConsumer(ReloadShelf);
				s.AddConsumerOf<ServiceContinued>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Started));
				s.AddConsumerOf<ServicePaused>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Paused));
				s.AddConsumerOf<ServiceStarting>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Starting));
				s.AddConsumerOf<ServiceStopping>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Stopping));
				s.AddConsumerOf<ShelfFault>().UsingConsumer(HandleShelfFault);
				s.AddConsumerOf<ServicePausing>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Pausing));
				s.AddConsumerOf<ServiceContinuing>().UsingConsumer(m => HandleShelfStateChange(m, ShelfState.Continuing));
            });

            _log.Debug("ShelfMaker started.");
        }

        ShelfHandle GetShelfStatus(string key)
        {
            return _shelves.UpgradeableReadLock(dict => dict.ContainsKey(key) ? dict[key] : null);
        }

        void ReloadShelf(ServiceMessage message)
        {
            try
            {
                _log.DebugFormat("Reloading shelf {0}", message.ShelfName);
                ShelfHandle shelf = GetShelfStatus(message.ShelfName);
                if (shelf != null)
                {
                    if (shelf.CurrentState == ShelfState.Started)
                    {
                        ManualResetEvent resetEvent = shelf.StopHandle = shelf.StopHandle ?? new ManualResetEvent(false);

                        StopShelf(message.ShelfName);

                        resetEvent.WaitOne(30.Seconds());
                    }

                    AppDomain.Unload(shelf.AppDomain);
                    _shelves.WriteLock(dict => dict.Remove(message.ShelfName));
                }

                MakeShelf(message.ShelfName);
            }
            catch (Exception ex)
            {
                // trash the partially formed shelf
                // another file system event might kick it up again
                _shelves.WriteLock(dict => { if (dict.ContainsKey(message.ShelfName)) dict.Remove(message.ShelfName); });
                _log.Error("Error reloading shelf '{0}'".FormatWith( message.ShelfName), ex);
            }
        }

        public void MakeShelf(string name, params AssemblyName[] assemblies)
        {
            MakeShelf(name, null, assemblies);
        }

        void MarkShelvedServiceStopped(ServiceStopped message)
        {
            ShelfHandle shelf = GetShelfStatus(message.ShelfName);
            if (shelf == null)
                return;

            if (shelf.StopHandle != null)
                shelf.StopHandle.Set();

            ShelfState oldState = shelf.CurrentState;
            shelf.CurrentState = ShelfState.Stopped;

            StateChanged(oldState, shelf.CurrentState, message.ShelfName);
        }

        void HandleShelfFault(ShelfFault message)
        {
            _log.Error("Error in shelf '{0}' -> '{1}'".FormatWith(message.ShelfName, message.Exception.Message), message.Exception);
            ShelfHandle shelfStatus = GetShelfStatus(message.ShelfName);
            if (shelfStatus == null)
                return;

            ReloadShelf(message);
        }

        void HandleShelfStateChange(ServiceMessage message, ShelfState newState)
        {
            ShelfHandle shelfStatus = GetShelfStatus(message.ShelfName);
            if (shelfStatus == null)
                return;

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = newState;

            StateChanged(oldState, newState, message.ShelfName);
        }

        public void MakeShelf(string name, Type bootstrapper, params AssemblyName[] assemblies)
        {
            _log.DebugFormat("Making shelf {0}", name);
            ShelfHandle shelf = GetShelfStatus(name);
            if (shelf != null)
            {
                _log.WarnFormat("Shelf '{0}' aleady exists. Cannot create.", name);
                throw new ArgumentException("Shelf already exists, cannot create a new one named: '{0}'".FormatWith(name));
            }

            AppDomainSetup settings = GetAppDomainSettings(name);

            AppDomain ad = AppDomain.CreateDomain(name, null, settings);

            assemblies.ToList().ForEach(x => ad.Load(x)); // add any missing assemblies
            Type shelfType = typeof(Shelf);

            if(_log.IsDebugEnabled)
            {
                var message = new StringBuilder();
                message.AppendFormat("Building shelf '{0}' ", name);
                if (bootstrapper != null)
                {
                    message.AppendFormat("with bootstrapper '{0}' ", bootstrapper.Name);
                    message.AppendFormat("in assembly '{0}'", bootstrapper.Assembly.GetName().Name);
                    message.AppendFormat(" - version: {0}", bootstrapper.Assembly.GetName().Version);
                }
                else
                {
                    message.AppendFormat("bootstrapper unknown - delegating to Shelf");
                }
                _log.Debug(message);
            }

            ObjectHandle shelfHandle = ad.CreateInstance(shelfType.Assembly.GetName().FullName, shelfType.FullName, true, 0, null, new object[] { bootstrapper },
                                                         null, null, null);

            _shelves.WriteLock(dict => dict.Add(name, new ShelfHandle
                                   {
                                       AppDomain = ad,
                                       ObjectHandle = shelfHandle, //TODO: if this is never used do we need to keep a reference?
                                       ShelfChannelBuilder = appDomain => WellknownAddresses.GetShelfChannelProxy(appDomain),
                                       ServiceChannelBuilder = appDomain => WellknownAddresses.GetServiceChannelProxy(appDomain, ad.FriendlyName),
                                       ShelfName = name
                                   }));

        }

        static AppDomainSetup GetAppDomainSettings(string name)
        {
            var settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";

            if (name == "TopShelf.DirectoryWatcher") return settings;

            settings.ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Services", name));
            _log.DebugFormat("Shelf[{0}].ApplicationBase = {1}", name, settings.ApplicationBase);
            settings.ConfigurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Services", Path.Combine(name, name + ".config")));
            _log.DebugFormat("Shelf[{0}].ConfigurationFile = {1}", name, settings.ConfigurationFile);
            return settings;
        }

        public ShelfState GetState(string shelfName)
        {
            ShelfHandle shelf = GetShelfStatus(shelfName);
            return shelf == null ? ShelfState.Unavailable : shelf.CurrentState;
        }

        public void StartShelf(string shelfName)
        {
            ShelfHandle shelf = GetShelfStatus(shelfName);
            if (shelf == null)
                return;

            shelf.ServiceChannel.Send(new StartService());
        }

        public void StopShelf(string shelfName)
        {
            ShelfHandle shelf = GetShelfStatus(shelfName);
            if (shelf == null)
                return;

            shelf.ServiceChannel.Send(new StopService());
        }

        void MarkServiceReadyAndStart(ServiceReady message)
        {
            ShelfHandle shelf = GetShelfStatus(message.ShelfName);
            if (shelf == null)
                return;

            ShelfState oldState = shelf.CurrentState;

            shelf.CurrentState = ShelfState.Ready;

            // if auto start is setup, send start
            shelf.ServiceChannel.Send(new StartService());

            StateChanged(oldState, ShelfState.Ready, message.ShelfName);
        }

        void MarkShelfReadyAndInitService(ShelfReady message)
        {
            ShelfHandle shelfStatus = GetShelfStatus(message.ShelfName);
            if (shelfStatus == null)
                return;

            ShelfState oldState = shelfStatus.CurrentState;

            shelfStatus.CurrentState = ShelfState.Readying;

            shelfStatus.ShelfChannel.Send(new ReadyService());

            StateChanged(oldState, ShelfState.Readying, message.ShelfName);
        }

        public delegate void ShelfStateChangedHandler(object sender, ShelfStateChangedEventArgs args);

        public event ShelfStateChangedHandler OnShelfStateChanged;

        void StateChanged(ShelfState oldState, ShelfState newState, string shelfName)
        {
            _log.DebugFormat("Shelf State Change '{0}' from '{1}' to '{2}'", shelfName, oldState, newState);
            ShelfStateChangedHandler handler = OnShelfStateChanged;
            if (handler != null)
            {
                handler(this, new ShelfStateChangedEventArgs { PreviousShelfState = oldState, CurrentShelfState = newState, ShelfName = shelfName });
            }
        }

        public void Dispose()
        {
        	int count = 0;
        	_shelves.ReadLock(dict => count = dict.Values.Count(x => x.CurrentState != ShelfState.Stopped));

			if (count > 0)
			{
				ManualResetEvent shutDown = new ManualResetEvent(false);
				CountdownLatch stopCountdown = new CountdownLatch(count, () =>
					{
						shutDown.Set();
					});
				OnShelfStateChanged += (sender, args) => {
					if (args.CurrentShelfState == ShelfState.Stopped)
						stopCountdown.CountDown();
				};
				
				_shelves.WriteLock(dict => dict.Values.ToList().ForEach(shelf => shelf.ShelfChannel.Send(new StopService())));

				shutDown.WaitOne(20.Seconds());
			}
            //how to wait for everything to have changed to stop + a timeout

            if (_myChannelHost != null)
            {
                _myChannelHost.Dispose();
            }

            _shelves.WriteLock(dict => dict.Values.ToList().ForEach(x => AppDomain.Unload(x.AppDomain)));
        }
    }

    public class ServiceMasterMind : 
        StateMachine<ServiceMasterMind>
    {
        #region State Machine
        static ServiceMasterMind()
        {
            Define(() =>
            {
                
            });
        }

        //do we have two kinds of shelves (in appdomain / out of appdomain)?
        public static Event ShelfIsReady { get; set; }
        public static Event ShelfFault { get; set; }
        public static Event FileSystemChange { get; set; }


        //service calls
        public static Event ServiceIsInitialized { get; set; }
        public static Event ServiceIsStarted { get; set; }
        public static Event ServiceIsStopped { get; set; }
        public static Event ServiceIsContinued { get; set; }
        public static Event ServiceIsPaused { get; set; }

        public static Event ServiceIsStarting { get; set; }
        public static Event ServiceIsStopping { get; set; }
        public static Event ServiceIsPausing { get; set; }
        public static Event ServiceIsContinuing { get; set; }

        public static State ServiceState { get; set; }
        #endregion
    }
}
