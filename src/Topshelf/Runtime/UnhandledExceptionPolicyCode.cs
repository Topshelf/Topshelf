// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Runtime
{
  using Hosts;

  public enum UnhandledExceptionPolicyCode
  {
    /// <summary>
    /// If an UnhandledException occurs, Topshelf will log an error and 
    /// stop the service
    /// </summary>
    LogErrorAndStopService = 0,
    /// <summary>
    /// If an UnhandledException occurs, Topshelf will log an error and 
    /// continue without stopping the service
    /// </summary>
    LogErrorOnly = 1,
    /// <summary>
    /// If an UnhandledException occurs, Topshelf will take no action. 
    /// It is assumed that the application will handle the UnhandledException itself.
    /// </summary>
    TakeNoAction = 2
  }
}