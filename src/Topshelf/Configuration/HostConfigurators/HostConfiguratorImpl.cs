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
    using Logging;
    using Runtime;
    using Runtime.Windows;

    public class HostConfiguratorImpl :
        HostConfigurator
    {
        static readonly Log _log = Logger.Get(typeof(HostConfiguratorImpl));

        readonly IList<HostBuilderConfigurator> _configurators;
        readonly WindowsHostSettings _settings;
        HostBuilderFactory _hostBuilderFactory;
        ServiceBuilderFactory _serviceBuilderFactory;

        public HostConfiguratorImpl()
        {
            _configurators = new List<HostBuilderConfigurator>();
            _settings = new WindowsHostSettings();

            _hostBuilderFactory = DefaultBuilderFactory;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_hostBuilderFactory == null)
                yield return this.Failure("HostBuilderFactory", "must not be null");

            if (_serviceBuilderFactory == null)
                yield return this.Failure("ServiceBuilderFactory", "must not be null");

            if (string.IsNullOrEmpty(_settings.DisplayName) && string.IsNullOrEmpty(_settings.Name))
                yield return this.Failure("DisplayName", "must be specified and not empty");

            if (string.IsNullOrEmpty(_settings.Name))
                yield return this.Failure("Name", "must be specified and not empty");
            else
            {
                var disallowed = new[] {' ', '\t', '\r', '\n', '\\', '/'};
                if (_settings.Name.IndexOfAny(disallowed) >= 0)
                    yield return this.Failure("Name", "must not contain whitespace, '/', or '\\' characters");
            }

            foreach (ValidateResult result in _configurators.SelectMany(x => x.Validate()))
            {
                yield return result;
            }

            yield return this.Success("Name", _settings.Name);
            
            if(_settings.Name != _settings.DisplayName)
                yield return this.Success("DisplayName", _settings.DisplayName);

            if(_settings.Name != _settings.Description)
                yield return this.Success("Description", _settings.Description);

            if(!string.IsNullOrEmpty(_settings.InstanceName))
                yield return this.Success("InstanceName", _settings.InstanceName);

            yield return this.Success("ServiceName", _settings.ServiceName);
        }

        public void SetDisplayName(string name)
        {
            _settings.DisplayName = name;
        }

        public void SetServiceName(string name)
        {
            _settings.Name = name;
        }

        public void SetDescription(string description)
        {
            _settings.Description = description;
        }

        public void SetInstanceName(string instanceName)
        {
            _settings.InstanceName = instanceName;
        }

        public void UseHostBuilder(HostBuilderFactory hostBuilderFactory)
        {
            _hostBuilderFactory = hostBuilderFactory;
        }

        public void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory)
        {
            _serviceBuilderFactory = serviceBuilderFactory;
        }

        public void AddConfigurator(HostBuilderConfigurator configurator)
        {
            _configurators.Add(configurator);
        }

        public Host CreateHost()
        {
            Type type = typeof(HostFactory);
            _log.InfoFormat("{0} v{1}, .NET Framework v{2}", type.Namespace, type.Assembly.GetName().Version,
                Environment.Version);

            ServiceBuilder serviceBuilder = _serviceBuilderFactory(_settings);

            HostBuilder builder = _hostBuilderFactory(_settings, serviceBuilder);

            foreach (HostBuilderConfigurator configurator in _configurators)
            {
                builder = configurator.Configure(builder);
            }

            return builder.Build();
        }

        static HostBuilder DefaultBuilderFactory(HostSettings settings, ServiceBuilder serviceBuilder)
        {
            return new RunBuilder(new WindowsHostEnvironment(), settings, serviceBuilder);
        }
    }
}