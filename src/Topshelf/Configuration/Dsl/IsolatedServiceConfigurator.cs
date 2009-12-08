// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf.Configuration.Dsl
{
    using System;
    using Model;

    public class IsolatedServiceConfigurator<TService> :
        ServiceConfiguratorBase<TService>,
        IIsolatedServiceConfigurator<TService>
        where TService : class
    {
        private string _pathToConfigurationFile;
        private string[] _args;
        private Func<AppDomainInitializer> _callback;


        public IServiceController Create()
        {
            IServiceController serviceController = new FacadeToIsolatedServiceController<TService>
                                                   {
                                                       StartAction = _startAction,
                                                       StopAction = _stopAction,
                                                       PauseAction = _pauseAction,
                                                       ContinueAction = _continueAction,
                                                       BuildAction  = _buildAction,
                                                       Name = _name,
                                                       PathToConfigurationFile = _pathToConfigurationFile,
                                                       Args = _args,
                                                       ConfigureArgsAction = _callback
                                                   };

            return serviceController;
        }

        public void ConfigurationFile(string pathToConfigurationFile)
        {
            _pathToConfigurationFile = pathToConfigurationFile;
        }

        public void CommandLineArguments(string[] args, Func<AppDomainInitializer> callback)
        {
            _args = args;
            _callback = callback;
        }
    }
}