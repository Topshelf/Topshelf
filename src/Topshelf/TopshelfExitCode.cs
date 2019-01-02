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
namespace Topshelf
{
	public enum TopshelfExitCode
	{
		Ok = 0,
		ServiceAlreadyInstalled = 1242,
		ServiceNotInstalled = 1243,
		ServiceAlreadyRunning = 1056,
		ServiceNotRunning = 1062,
		ServiceControlRequestFailed = 1064,
		AbnormalExit = 1067,
		SudoRequired = 2,
		NotRunningOnWindows = 11
	}
}
