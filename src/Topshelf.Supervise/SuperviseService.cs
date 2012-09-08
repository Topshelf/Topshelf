// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Supervise
{
    using System;
    using System.Linq;
    using System.Threading;
    using HostConfigurators;
    using Runtime;
    using Scripting;
    using Scripting.Commands;
    using Threading;

    public class SuperviseService :
        ServiceControl,
        HostControl,
        CommandHandler,
        IDisposable
    {
        readonly CommandHandler[] _commandHandlers;
        readonly Fiber _fiber;
        readonly Scheduler _scheduler;
        readonly ServiceAvailability _serviceAvailability;
        readonly ServiceBuilderFactory _serviceBuilderFactory;
        readonly HostSettings _settings;
        readonly TimeSpan _shutdownTimeout = TimeSpan.FromSeconds(30);

        bool _disposed;
        HostControl _hostControl;
        TimeSpan _monitorInterval = TimeSpan.FromSeconds(10);
        ServiceHandle _serviceHandle;

        public SuperviseService(HostSettings settings, ServiceAvailability serviceAvailability,
            ServiceBuilderFactory serviceBuilderFactory)
        {
            _settings = settings;
            _serviceAvailability = serviceAvailability;
            _serviceBuilderFactory = serviceBuilderFactory;

            _fiber = new PoolFiber();
            _scheduler = new TimerScheduler(new PoolFiber());

            _commandHandlers = CreateCommandHandlers();
        }

        bool CommandHandler.Handle(Guid commandId, CommandScript script)
        {
            if (commandId == Guid.Empty)
                return true;

            return _commandHandlers.Any(handler => handler.Handle(commandId, script));
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // this is for US
        }

        void HostControl.Stop()
        {
            StopService();
        }

        void HostControl.Restart()
        {
            RestartService();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool ServiceControl.Start(HostControl hostControl)
        {
            _hostControl = hostControl;

            _fiber.Add(MonitorService);

            return true;
        }

        bool ServiceControl.Stop(HostControl hostControl)
        {
            var mre = new ManualResetEvent(false);
            _fiber.Add(StopService);
            _fiber.Add(() => mre.Set());

            mre.WaitOne(_shutdownTimeout);
            _fiber.Add(() =>
                {
                    using (mre)
                    {
                    }
                });

            return _serviceHandle == null;
        }

        ~SuperviseService()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _scheduler.Stop(_shutdownTimeout);
                _fiber.Shutdown(_shutdownTimeout);
            }

            _disposed = true;
        }

        void StartService()
        {
            if (_serviceHandle != null)
                return;

            if (!_serviceAvailability.CanStart())
                return;

            var arguments = new CommandScriptStepArguments {_serviceBuilderFactory};

            var script = new CommandScript
                {
                    new CommandScriptStep<CreateServiceCommand>(arguments),
                    new CommandScriptStep<StartServiceCommand>(arguments),
                };

            bool started = Execute(script);
            if (started)
            {
                _serviceHandle = arguments.Get<ServiceHandle>();
            }
        }

        void StopService()
        {
            var unloadArguments = new CommandScriptStepArguments
                {
                    _serviceHandle,
                };

            var script = new CommandScript
                {
                    new CommandScriptStep<StopServiceCommand>(unloadArguments),
                    new CommandScriptStep<UnloadServiceCommand>(unloadArguments),
                };

            bool stopped = Execute(script);
            if (stopped)
            {
                _serviceHandle = null;
            }
        }

        bool RestartService()
        {
            var createArguments = new CommandScriptStepArguments
                {
                    _serviceBuilderFactory,
                };

            var unloadArguments = new CommandScriptStepArguments
                {
                    _serviceHandle,
                };

            var script = new CommandScript
                {
                    new CommandScriptStep<CreateServiceCommand>(createArguments),
                    new CommandScriptStep<StopServiceCommand>(unloadArguments),
                    new CommandScriptStep<StartServiceCommand>(createArguments),
                    new CommandScriptStep<UnloadServiceCommand>(unloadArguments),
                };

            bool restarted = Execute(script);
            if (restarted)
            {
                _serviceHandle = createArguments.Get<ServiceHandle>();
            }
            return restarted;
        }

        CommandHandler[] CreateCommandHandlers()
        {
            return new CommandHandler[]
                {
                    new CommandHandler<CreateServiceCommand>(this),
                    new CommandHandler<StartServiceCommand>(this),
                    new CommandHandler<StopServiceCommand>(this),
                    new CommandHandler<UnloadServiceCommand>(this),
                };
        }

        bool Execute(CommandScript script)
        {
            script.Variables.Add(_settings);
            script.Variables.Add(_hostControl);

            return ((CommandHandler)this).Handle(script.NextCommandId, script);
        }

        void MonitorService()
        {
            try
            {
                if (_serviceHandle == null)
                    _fiber.Add(StartService);
            }
            finally
            {
                _scheduler.Schedule(_monitorInterval, _fiber, MonitorService);
            }
        }
    }
}