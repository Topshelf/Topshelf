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
    using Commands;
    using HostConfigurators;
    using Runtime;

    public class SuperviseHost :
        SuperviseHostControl,
        CommandHandler
    {
        readonly HostControl _hostControl;
        readonly ServiceBuilderFactory _serviceBuilderFactory;
        readonly HostSettings _settings;

        public SuperviseHost(HostControl hostControl, HostSettings settings, ServiceBuilderFactory serviceBuilderFactory)
        {
            _hostControl = hostControl;
            _settings = settings;
            _serviceBuilderFactory = serviceBuilderFactory;

            _commandHandlers = CreateCommandHandlers();
        }

        public void RequestAdditionalTime(TimeSpan timeRemaining)
        {
        }

        public void Stop()
        {
            // if the service ASKS to stop, we're going to stop it.
            _hostControl.Stop();
        }

        public void Restart()
        {
        }


        public void Start()
        {
            var arguments = new CommandScriptStepArguments {_hostControl, _settings, _serviceBuilderFactory};

            var script = new CommandScript
            {
                new CommandScriptStep<CreateServiceCommand>(arguments),
                new CommandScriptStep<StartServiceCommand>(new CommandScriptStepArguments{_hostControl}),
            };

            script.Variables.Add(_serviceHandle);

            if (Execute(script))
            {
                _serviceHandle = script.Variables.Get<ServiceHandle>();
            }
        }

        public void Create()
        {
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        readonly CommandHandler[] _commandHandlers;
        ServiceHandle _serviceHandle;

        CommandHandler[] CreateCommandHandlers()
        {
            return new CommandHandler[]
                {
                    new CommandHandler<CreateServiceCommand>(this),
                    new CommandHandler<StartServiceCommand>(this),
                    new CommandHandler<StopServiceCommand>(this),
                };
        }

        bool CommandHandler.Handle(Guid commandId, CommandScript commandScript)
        {
            return _commandHandlers.Any(handler => handler.Handle(commandId, commandScript));
        }

        bool Execute(CommandScript commandScript)
        {
            return ((CommandHandler)this).Handle(commandScript.NextCommandId, commandScript);
        }

    }
}