// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using CommandLineParser;
    using Configurators;
    using Logging;
    using Options;
    using Runtime;
    // using Runtime.Windows;


    public class HostConfiguratorImpl :
        HostConfigurator,
        Configurator
    {
        readonly IList<CommandLineConfigurator> _commandLineOptionConfigurators;
        readonly IList<HostBuilderConfigurator> _configurators;
        HostSettings _settings;
        bool _commandLineApplied;
        // EnvironmentBuilderFactory _environmentBuilderFactory;
        HostBuilderFactory _hostBuilderFactory;
        ServiceBuilderFactory _serviceBuilderFactory;

        public HostConfiguratorImpl()
        {
            _configurators = new List<HostBuilderConfigurator>();
            _commandLineOptionConfigurators = new List<CommandLineConfigurator>();
            
            // _environmentBuilderFactory = DefaultEnvironmentBuilderFactory;
            _hostBuilderFactory = DefaultHostBuilderFactory;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_hostBuilderFactory == null)
                yield return this.Failure("HostBuilderFactory", "must not be null");

            if (_serviceBuilderFactory == null)
                yield return this.Failure("ServiceBuilderFactory", "must not be null");

            // if (_environmentBuilderFactory == null)
            //     yield return this.Failure("EnvironmentBuilderFactory", "must not be null");

            foreach (ValidateResult result in _configurators.SelectMany(x => x.Validate()))
                yield return result;
        }

        public void UseHostBuilder(HostBuilderFactory hostBuilderFactory)
        {
            _hostBuilderFactory = hostBuilderFactory;
        }

        public void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory)
        {
            _serviceBuilderFactory = serviceBuilderFactory;
        }

        public void UseEnvironmentBuilder(EnvironmentBuilderFactory environmentBuilderFactory)
        {
            // _environmentBuilderFactory = environmentBuilderFactory;
        }

        public void AddConfigurator(HostBuilderConfigurator configurator)
        {
            _configurators.Add(configurator);
        }

        public void ApplyCommandLine()
        {
            if (_commandLineApplied)
                return;

            IEnumerable<Option> options = CommandLine.Parse<Option>(ConfigureCommandLineParser);
            ApplyCommandLineOptions(options);
        }

        public void ApplyCommandLine(string commandLine)
        {
            IEnumerable<Option> options = CommandLine.Parse<Option>(ConfigureCommandLineParser, commandLine);
            ApplyCommandLineOptions(options);

            _commandLineApplied = true;
        }

        public void AddCommandLineSwitch(string name, Action<bool> callback)
        {
            var configurator = new CommandLineSwitchConfigurator(name, callback);

            _commandLineOptionConfigurators.Add(configurator);
        }

        public void AddCommandLineDefinition(string name, Action<string> callback)
        {
            var configurator = new CommandLineDefinitionConfigurator(name, callback);

            _commandLineOptionConfigurators.Add(configurator);
        }

        public Host CreateHost()
        {
            Type type = typeof(HostFactory);
            HostLogger.Get<HostConfiguratorImpl>()
                      .InfoFormat("{0}", type.Namespace);

            // EnvironmentBuilder environmentBuilder = _environmentBuilderFactory(this);

            // HostEnvironment environment = environmentBuilder.Build();

            // ServiceBuilder serviceBuilder = _serviceBuilderFactory(_settings);

            // HostBuilder builder = _hostBuilderFactory(environment, _settings);

            // foreach (HostBuilderConfigurator configurator in _configurators)
            //     builder = configurator.Configure(builder);

            // return builder.Build(serviceBuilder);
            return null;
        }

        void ApplyCommandLineOptions(IEnumerable<Option> options)
        {
            foreach (Option option in options)
                option.ApplyTo(this);
        }

        void ConfigureCommandLineParser(ICommandLineElementParser<Option> parser)
        {
            CommandLineParserOptions.AddTopshelfOptions(parser);

            foreach (CommandLineConfigurator optionConfigurator in _commandLineOptionConfigurators)
                optionConfigurator.Configure(parser);

            CommandLineParserOptions.AddUnknownOptions(parser);
        }

        static HostBuilder DefaultHostBuilderFactory(HostEnvironment environment, HostSettings settings)
        {
            return new RunBuilder(environment, settings);
        }

        // static EnvironmentBuilder DefaultEnvironmentBuilderFactory(HostConfigurator configurator)
        // {
        //     return new WindowsHostEnvironmentBuilder(configurator);
        // }
    }
}