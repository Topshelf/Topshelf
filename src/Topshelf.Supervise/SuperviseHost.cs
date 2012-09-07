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
    using Builders;
    using Commands;
    using HostConfigurators;
    using Runtime;

    public class SuperviseHost :
        HostControl
    {
        readonly HostControl _hostControl;
        readonly ServiceBuilderFactory _serviceBuilder;
        readonly HostSettings _settings;

        public SuperviseHost(HostControl hostControl, HostSettings settings, ServiceBuilderFactory serviceBuilder)
        {
            _hostControl = hostControl;
            _settings = settings;
            _serviceBuilder = serviceBuilder;
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


        public void StartService()
        {
            var arguments = new CommandTaskArguments {_hostControl, _settings, _serviceBuilder};

            var workList = new WorkList(new CommandTask[]
                {
                    new CommandTask<StartServiceCommand>(arguments),
                });

            var handler = new DispatchCommandHandler(workList, x => new CommandHandler<StartServiceCommand>(x));
            handler.Execute();
        }
    }
}