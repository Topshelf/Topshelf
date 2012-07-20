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
    public class ServiceRecoveryOptions
    {
        public ServiceRecoveryOptions()
        {
            FirstFailureAction = ServiceRecoveryAction.TakeNoAction;
            SecondFailureAction = ServiceRecoveryAction.TakeNoAction;
            SubsequentFailureAction = ServiceRecoveryAction.TakeNoAction;
            ResetFailureCountWaitDays = 0;
            RestartServiceWaitMinutes = 1;
            RestartSystemWaitMinutes = 1;
        }

        public ServiceRecoveryAction FirstFailureAction { get; set; }
        public ServiceRecoveryAction SecondFailureAction { get; set; }
        public ServiceRecoveryAction SubsequentFailureAction { get; set; }
        public int ResetFailureCountWaitDays { get; set; }
        public int RestartServiceWaitMinutes { get; set; }
        public int RestartSystemWaitMinutes { get; set; }
        public string RestartSystemMessage { get; set; }
        public string RunProgramCommand { get; set; }
        public string RunProgramParameters { get; set; }
    }
}