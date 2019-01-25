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

    /// <summary>
    /// Implements a service recovery configurator and host builder configurator.
    /// </summary>
    /// <seealso cref="Topshelf.ServiceRecoveryConfigurator" />
    /// <seealso cref="Topshelf.HostConfigurators.HostBuilderConfigurator" />
    public class ServiceRecoveryHostConfigurator :
        ServiceRecoveryConfigurator,
        HostBuilderConfigurator
    {
        ServiceRecoveryOptions _options;
        HostSettings _settings;

        ServiceRecoveryOptions Options => _options ?? (_options = new ServiceRecoveryOptions());

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

        /// <summary>
        /// Configures the host builder.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <returns>The configured host builder.</returns>
        /// <exception cref="ArgumentNullException">builder</exception>
        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            _settings = builder.Settings;

            builder.Match<InstallBuilder>(x => x.AfterInstall(ConfigureServiceRecovery));

            return builder;
        }

        /// <summary>
        /// Adds a restart service recovery action with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns>The service recovery configurator.</returns>
        public ServiceRecoveryConfigurator RestartService(TimeSpan delay)
        {
            Options.AddAction(new RestartServiceRecoveryAction(delay));

            return this;
        }

        /// <summary>
        /// Adds a restart service recovery action with the specified delay in minutes.
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <returns>The service recovery configurator.</returns>
        public ServiceRecoveryConfigurator RestartService(int delayInMinutes)
        {
            return RestartService(TimeSpan.FromMinutes(delayInMinutes));
        }

        /// <summary>
        /// Adds a restart computer recovery action with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="message">The message.</param>
        /// <returns>ServiceRecoveryConfigurator.</returns>
        public ServiceRecoveryConfigurator RestartComputer(TimeSpan delay, string message)
        {
            Options.AddAction(new RestartSystemRecoveryAction(delay, message));

            return this;
        }

        /// <summary>
        /// Adds a restart computer recovery action with the specified delay in minutes.
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <param name="message">The message.</param>
        /// <returns>The service recovery configurator.</returns>
        public ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message)
        {
            return RestartComputer(TimeSpan.FromMinutes(delayInMinutes), message);
        }

        /// <summary>
        /// Adds a run program recovery action with the specified delay.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <param name="command">The command to run.</param>
        /// <returns>The service recovery configurator.</returns>
        public ServiceRecoveryConfigurator RunProgram(TimeSpan delay, string command)
        {
            Options.AddAction(new RunProgramRecoveryAction(delay, command));

            return this;
        }

        /// <summary>
        /// Adds a run program recovery action with the specified delay in minutes.
        /// </summary>
        /// <param name="delayInMinutes">The delay in minutes.</param>
        /// <param name="command">The command.</param>
        /// <returns>The service recovery configurator.</returns>
        public ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command)
        {
            return RunProgram(TimeSpan.FromMinutes(delayInMinutes), command);
        }

        /// <summary>
        /// Adds a take no action recovery action.
        /// </summary>
        /// <returns>The service recovery configurator.</returns>
        public ServiceRecoveryConfigurator TakeNoAction()
        {
            Options.AddAction(new TakeNoActionAction());

            return this;
        }

        /// <summary>
        /// Sets the recovery reset period in days.
        /// </summary>
        /// <param name="days">The reset period in days.</param>
        public ServiceRecoveryConfigurator SetResetPeriod(int days)
        {
            Options.ResetPeriod = days;

            return this;
        }

        /// <summary>
        /// Specifies that the recovery actions should only be taken on a service crash. If the service exits
        /// with a non-zero exit code, it will not be restarted.
        /// </summary>
        public void OnCrashOnly()
        {
            Options.RecoverOnCrashOnly = true;
        }

        void ConfigureServiceRecovery(InstallHostSettings installSettings)
        {
            var controller = new WindowsServiceRecoveryController();
            controller.SetServiceRecoveryOptions(installSettings, _options);
        }
    }
}
