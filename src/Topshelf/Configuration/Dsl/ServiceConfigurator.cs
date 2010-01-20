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
    using Model;

    public class ServiceConfigurator<TService> :
        ServiceConfiguratorBase<TService>,
        IServiceConfigurator<TService>
        where TService : class
    {
        public IServiceController Create()
        {
            IServiceController serviceController = new ServiceController<TService>
                                                   {
                                                       StartAction = _startAction,
                                                       StopAction = _stopAction,
                                                       PauseAction = _pauseAction,
                                                       ContinueAction = _continueAction,
                                                       BuildService = _buildAction,
                                                       Name = _name,
                                                   };

            return serviceController;
        }


    }
}