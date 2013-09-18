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
    using System.ServiceProcess;
    using Builders;
    using Configurators;

    public class RunAsUserHostConfigurator :
        HostBuilderConfigurator
    {
        public RunAsUserHostConfigurator(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public string Password { get; private set; }
        public string Username { get; private set; }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.RunAs(this.Username, this.Password, ServiceAccount.User));

            return builder;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (string.IsNullOrEmpty(this.Username))
                yield return this.Failure("Username", "must be specified for a User account type");
            if (string.IsNullOrEmpty(this.Password))
                yield return this.Failure("Password", "must be specified for a User account type");
        }
    }
}