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
namespace Topshelf.HostConfigurators
{
    using System.Collections.Generic;
    using Builders;
    using Configurators;


    public class CommandConfigurator :
        HostBuilderConfigurator
    {
        readonly int _command;

        public CommandConfigurator(int command)
        {
            _command = command;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_command < 128 || _command > 256)
                yield return this.Failure("Command", "must be between 128 and 256");
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            return new CommandBuilder(builder, _command);
        }
    }
}