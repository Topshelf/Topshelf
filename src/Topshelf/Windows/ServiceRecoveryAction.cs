// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Windows
{
	// Henrik: I removed uint from this enum in order to remove a CLSCompiant warning. It's now 'int', the default.
	// but according to Find All Usages, it's only ever used for switching anyway. 
	// Besides, we're not above log2(2^31) number of mutually exclusive options, or 2^31 options in this enum anyway, 
	// so it should work with int, yeah?

	public enum ServiceRecoveryAction
	{
		TakeNoAction,
		RestartService,
		RestartComputer,
		RunProgram
	}
}