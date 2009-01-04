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
namespace Topshelf.Configuration
{
    using System;
    using System.Windows.Forms;

    public interface IHostConfigurator : IDisposable
    {
        void ConfigureService<TService>();
        void ConfigureService<TService>(Action<IServiceConfigurator<TService>> action);

        void SetDisplayName(string displayName);
        void SetServiceName(string serviceName);
        void SetDescription(string description);

        /// <summary>
        /// We set the service to start automatically by default. This sets the service to manual instead.
        /// </summary>
        void DoNotStartAutomatically();

        void RunAsLocalSystem();
        void RunAs(string username, string password);
        void RunAsFromInteractive();

        void DependsOn(string serviceName);
        void DependencyOnMsmq();
        void DependencyOnMsSql();

        /// <summary>
        /// By default we use a console/winservice host. By executing this method you state you prefer the winformhost.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void UseWinFormHost<T>() where T : Form;

        void BeforeStart(Action<IServiceCoordinator> action);
    }
}