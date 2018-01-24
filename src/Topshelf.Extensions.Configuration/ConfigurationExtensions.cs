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

    public static class ConfigurationExtensions
    {
        public static void ApplyConfiguration(this HostConfigurator configurator, IConfiguration configuration)
        {
            var options = Parse(configuration);

            configurator.ApplyOptions(options);
        }

        public static IEnumerable<Option> Parse(IConfiguration configuration)
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
#if !NETCORE
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
                    if ((bool)TypeDescriptor.GetConverter(typeof(bool)).ConvertFromInvariantString(entry.Value))
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
#endif
            }

            return options;
        }

        public static void ApplyOptions(this HostConfigurator configurator, IEnumerable<Option> options)
        {
            foreach (var option in options)
            {
                option.ApplyTo(configurator);
            }
        }
    }
}
