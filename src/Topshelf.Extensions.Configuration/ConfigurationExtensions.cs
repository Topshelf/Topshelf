// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Extensions.Configuration;
    using Topshelf.HostConfigurators;
    using Topshelf.Options;
    using Topshelf.Runtime.Windows;

    /// <summary>
    /// Provides Topshelf extensions for Microsoft extensions for configuration.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        /// <param name="configuration">The configuration.</param>
        public static void ApplyConfiguration(this HostConfigurator configurator, IConfiguration configuration)
            => configurator.ApplyConfiguration(configuration.GetTopshelfSection());

        /// <summary>
        /// Applies the configuration.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        /// <param name="configuration">The configuration section.</param>
        public static void ApplyConfiguration(this HostConfigurator configurator, IConfigurationSection configuration)
        {
            var options = configuration.Parse();

            configurator.ApplyOptions(options);
        }

        /// <summary>
        /// Parses the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>A list of configuration options.</returns>
        public static IEnumerable<Option> Parse(this IConfiguration configuration)
            => configuration.GetTopshelfSection().Parse();

        /// <summary>
        /// Parses the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration section.</param>
        /// <returns>A list of configuration options.</returns>
        public static IEnumerable<Option> Parse(this IConfigurationSection configuration)
        {
            var options = new List<Option>();

            foreach (var entry in configuration.AsEnumerable(true))
            {
                if (string.Equals(entry.Key, "SystemOnly", StringComparison.OrdinalIgnoreCase))
                {
                    if ((bool)TypeDescriptor.GetConverter(typeof(bool)).ConvertFromInvariantString(entry.Value))
                    {
                        options.Add(new SystemOnlyHelpOption());
                    }
                }
                else if (string.Equals(entry.Key, "ServiceName", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new ServiceNameOption(entry.Value));
                }
                else if (string.Equals(entry.Key, "Description", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new ServiceDescriptionOption(entry.Value));
                }
                else if (string.Equals(entry.Key, "DisplayName", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new DisplayNameOption(entry.Value));
                }
                else if (string.Equals(entry.Key, "Instance", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new InstanceOption(entry.Value));
                }
                else if (string.Equals(entry.Key, "StartTimeout", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new StartTimeoutOption(configuration.GetSection("StartTimeout").Get<int>()));
                }
                else if (string.Equals(entry.Key, "StopTimeout", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new StopTimeoutOption(configuration.GetSection("StopTimeout").Get<int>()));
                }
                else if (string.Equals(entry.Key, "StartMode", StringComparison.OrdinalIgnoreCase))
                {
                    var startMode = entry.Value;

                    if (string.Equals(startMode, "Automatic", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(startMode, "auto", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new AutostartOption());
                    }
                    else if (string.Equals(startMode, "Manual", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new ManualStartOption());
                    }
                    else if (string.Equals(startMode, "Disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new DisabledOption());
                    }
                    else if (string.Equals(startMode, "Delayed", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new DelayedOption());
                    }
                    else
                    {
                        throw new HostConfigurationException("The service was not properly configured");
                    }
                }
                else if (string.Equals(entry.Key, "Interactive", StringComparison.OrdinalIgnoreCase))
                {
                    if (configuration.GetSection("Interactive").Get<bool>())
                    {
                        options.Add(new InteractiveOption());
                    }
                }
                else if (string.Equals(entry.Key, "Account", StringComparison.OrdinalIgnoreCase))
                {
                    var account = configuration.GetSection("Account");
                    var accountValue = account.Value;

                    if (string.Equals(accountValue, "LocalSystem", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new LocalSystemOption());
                    }
                    else if (string.Equals(accountValue, "LocalService", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new LocalServiceOption());
                    }
                    else if (string.Equals(accountValue, "NetworkService", StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(new NetworkServiceOption());
                    }
                    else if (accountValue == null)
                    {
                        options.Add(
                            new ServiceAccountOption(
                                configuration.GetSection("account:username").Value,
                                configuration.GetSection("account:password").Value));
                    }
                    else
                    {
                        throw new HostConfigurationException("The service was not properly configured");
                    }
                }
                else if (string.Equals(entry.Key, "Dependencies", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var dependency in configuration.GetSection("Dependencies").GetChildren())
                    {
                        options.Add(new DependencyOption(dependency.Value));
                    }
                }
                else if (string.Equals(entry.Key, "ServiceRecovery", StringComparison.OrdinalIgnoreCase))
                {
                    var section = configuration.GetSection("ServiceRecovery");

                    var serviceRecoveryOptions = new ServiceRecoveryOptions
                    {
                        RecoverOnCrashOnly = section.GetSection(nameof(ServiceRecoveryOptions.RecoverOnCrashOnly)).Get<bool>(),
                        ResetPeriod = section.GetSection(nameof(ServiceRecoveryOptions.ResetPeriod)).Get<int>()
                    };

                    foreach (var recoveryActionSecion in section.GetSection("RecoveryActions")?.GetChildren())
                    {
                        var recoveryActionType = recoveryActionSecion.GetSection("Type")?.Value;

                        switch (recoveryActionType)
                        {
                            case "RestartService":
                                serviceRecoveryOptions.AddAction(
                                    new RestartServiceRecoveryAction(
                                        TimeSpan.FromMinutes(
                                            recoveryActionSecion.GetSection("Delay").Get<int>())));
                                break;
                            case "RestartSystem":
                                serviceRecoveryOptions.AddAction(
                                    new RestartSystemRecoveryAction(
                                        TimeSpan.FromMinutes(
                                            recoveryActionSecion.GetSection("Delay").Get<int>()),
                                            recoveryActionSecion.GetSection("Message").Value));
                                break;
                            case "RunProgram":
                                serviceRecoveryOptions.AddAction(
                                    new RunProgramRecoveryAction(
                                        TimeSpan.FromMinutes(
                                            recoveryActionSecion.GetSection("Delay").Get<int>()),
                                            recoveryActionSecion.GetSection("Command").Value));
                                break;
                        }
                    }

                    options.Add(new ServiceRecoveryOption(serviceRecoveryOptions));
                }
            }

            return options;
        }

        /// <summary>
        /// Applies the configuration options.
        /// </summary>
        /// <param name="configurator">The host configurator.</param>
        /// <param name="options">The configuration options.</param>
        public static void ApplyOptions(this HostConfigurator configurator, IEnumerable<Option> options)
        {
            foreach (var option in options)
            {
                option.ApplyTo(configurator);
            }
        }

        /// <summary>
        /// Gets the default Topshelf configuration section.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The default Topshelf configuration section.</returns>
        public static IConfigurationSection GetTopshelfSection(this IConfiguration configuration)
            => configuration.GetSection("Topshelf");
    }
}
