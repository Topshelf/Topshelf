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
    using System.ServiceProcess;
    using Internal;
    using Internal.Actions;

    public class RunConfiguration :
        IRunConfiguration
    {
        public RunConfiguration()
        {
            DefaultAction = NamedAction.Console;
        }

        public WinServiceSettings WinServiceSettings { get; set; }
        public Credentials Credentials { get; set; }
        public IServiceCoordinator Coordinator { get; set; }
        public Type FormType { get; private set; }
        public NamedAction DefaultAction { get; private set; }

        public void SetRunnerAction(NamedAction action, Type form)
        {
            DefaultAction = action;
            FormType = form;
        }

        public virtual void ConfigureServiceInstaller(ServiceInstaller installer)
        {
            installer.ServiceName = WinServiceSettings.FullServiceName;
            installer.Description = WinServiceSettings.Description;
            installer.DisplayName = WinServiceSettings.FullDisplayName;
            installer.ServicesDependedOn = WinServiceSettings.Dependencies.ToArray();
            installer.StartType = ServiceStartMode.Automatic;
        }
        public virtual void ConfigureServiceProcessInstaller(ServiceProcessInstaller installer)
        {
            installer.Username = Credentials.Username;
            installer.Password = Credentials.Password;
            installer.Account = Credentials.AccountType;
        }
    }
}