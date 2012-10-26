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
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using Configurators;
    using Runtime;
#if !LINUX
    using Runtime.Windows;
#else
	using Runtime.Linux;
#endif

    public class ServiceRecoveryHostConfigurator :
        ServiceRecoveryConfigurator,
        HostBuilderConfigurator
    {
        ServiceRecoveryOptions _options;
        HostSettings _settings;

        ServiceRecoveryOptions Options
        {
            get { return _options ?? (_options = new ServiceRecoveryOptions()); }
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_options == null)
                yield return this.Failure("No service recovery options were specified");
            else
            {
                if (!_options.Actions.Any())
                    yield return this.Failure("No service recovery actions were specified.");

                if (_options.Actions.Count(x => x.GetType() == typeof(RestartServiceRecoveryAction)) > 1)
                    yield return this.Failure("Only a single restart service action can be specified.");

                if (_options.Actions.Count(x => x.GetType() == typeof(RestartSystemRecoveryAction)) > 1)
                    yield return this.Failure("Only a single restart system action can be specified.");

                if (_options.Actions.Count(x => x.GetType() == typeof(RunProgramRecoveryAction)) > 1)
                    yield return this.Failure("Only a single run program action can be specified.");
            }
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            _settings = builder.Settings;

            builder.Match<InstallBuilder>(x => x.AfterInstall(ConfigureServiceRecovery));

            return builder;
        }

        public ServiceRecoveryConfigurator RestartService(int delayInMinutes)
        {
            Options.AddAction(new RestartServiceRecoveryAction(delayInMinutes));

            return this;
        }

        public ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message)
        {
            Options.AddAction(new RestartSystemRecoveryAction(delayInMinutes, message));

            return this;
        }

        public ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command)
        {
            Options.AddAction(new RunProgramRecoveryAction(delayInMinutes, command));

            return this;
        }

        public ServiceRecoveryConfigurator SetResetPeriod(int days)
        {
            Options.ResetPeriod = days;

            return this;
        }

        void ConfigureServiceRecovery(InstallHostSettings installSettings)
        {
            var controller = new ServiceRecoveryController();
            controller.SetServiceRecoveryOptions(_settings, _options);
        }
    }
}