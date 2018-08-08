﻿// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Runtime.Windows
{
    using System.Collections.Generic;

    public class ServiceRecoveryOptions
    {
        readonly IList<ServiceRecoveryAction> _actions;

        public ServiceRecoveryOptions()
        {
            _actions = new List<ServiceRecoveryAction>();
        }

        public int ResetPeriod { get; set; }

        public IEnumerable<ServiceRecoveryAction> Actions => _actions;

        public bool RecoverOnCrashOnly { get; set; }

        public void AddAction(ServiceRecoveryAction serviceRecoveryAction)
        {
            if (_actions.Count == 3)
            {
                throw new TopshelfException("Recovery action can not be added. " +
                                            "Windows recovery does not support more than 3 actions");
            }

            _actions.Add(serviceRecoveryAction);
        }
    }
}