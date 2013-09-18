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
    using Builders;
    using Configurators;
    using Runtime;

    public class StartModeHostConfigurator :
        HostBuilderConfigurator
    {
        public StartModeHostConfigurator(HostStartMode startMode)
        {
            this.StartMode = startMode;
        }

        public HostStartMode StartMode { get; private set; }

        public IEnumerable<ValidateResult> Validate()
        {
#if NET35
            if (_startMode == HostStartMode.AutomaticDelayed)
                yield return this.Failure("StartMode", "Automatic (Delayed) is only available on .NET 4.0 or later");
#endif
            yield break;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.SetStartMode(this.StartMode));

            return builder;
        }
    }
}