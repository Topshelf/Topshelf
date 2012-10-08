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
namespace Topshelf.Runtime.Windows
{
    using System;
    
    public class RestartSystemRecoveryAction :
        ServiceRecoveryAction
    {
        public RestartSystemRecoveryAction(int delay, string restartMessage)
            : base(delay)
        {
            RestartMessage = restartMessage;
        }

        public string RestartMessage { get; private set; }

        public override NativeMethods.SC_ACTION GetAction()
        {
            return new NativeMethods.SC_ACTION
                {
                    Delay = (int)TimeSpan.FromMinutes(DelayInMinutes).TotalMilliseconds,
                    Type = (int)NativeMethods.SC_ACTION_TYPE.RebootComputer,
                };
        }
    }
}