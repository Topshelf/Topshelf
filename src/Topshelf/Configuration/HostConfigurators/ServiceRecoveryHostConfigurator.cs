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
    using Runtime.Windows;

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

        public ServiceRecoveryConfigurator RestartService(TimeSpan delay)
        {
            Options.AddAction(new RestartServiceRecoveryAction(delay));

            return this;
        }

        public ServiceRecoveryConfigurator RestartService(int delayInMinutes)
        {
            return RestartService(TimeSpan.FromMinutes(delayInMinutes));
        }

        public ServiceRecoveryConfigurator RestartComputer(TimeSpan delay, string message)
        {
            Options.AddAction(new RestartSystemRecoveryAction(delay, message));

            return this;
        }

        public ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message)
        {
            return RestartComputer(TimeSpan.FromMinutes(delayInMinutes), message);
        }

        public ServiceRecoveryConfigurator RunProgram(TimeSpan delay, string command)
        {
            Options.AddAction(new RunProgramRecoveryAction(delay, command));

            return this;
        }

        public ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command)
        {
            return RunProgram(TimeSpan.FromMinutes(delayInMinutes), command);
        }

        public ServiceRecoveryConfigurator SetResetPeriod(int days)
        {
            Options.ResetPeriod = days;

            return this;
        }

        public void OnCrashOnly()
        {
            Options.RecoverOnCrashOnly = true;
        }

        void ConfigureServiceRecovery(InstallHostSettings installSettings)
        {
            var controller = new WindowsServiceRecoveryController();
            controller.SetServiceRecoveryOptions(_settings, _options);
        }
    }
}