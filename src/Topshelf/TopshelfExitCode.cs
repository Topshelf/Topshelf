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
    public struct TopshelfExitCode
    {
        private readonly int _exitCode;

        public TopshelfExitCode(int exitCode)
        {
            _exitCode = exitCode;
        }
        
        public static explicit operator int(TopshelfExitCode topshelfExitCode)
        {
            return topshelfExitCode._exitCode;
        }

        // windows service exit code compliant 
        // https://docs.microsoft.com/en-us/windows/desktop/Debug/system-error-codes
        public static TopshelfExitCode Ok { get; } = new TopshelfExitCode(0);
        public static TopshelfExitCode ServiceAlreadyInstalled { get; } = new TopshelfExitCode(1242);
        public static TopshelfExitCode ServiceNotInstalled { get; } = new TopshelfExitCode(1243);
        public static TopshelfExitCode ServiceAlreadyRunning { get; } = new TopshelfExitCode(1056);
        public static TopshelfExitCode ServiceNotRunning { get; } = new TopshelfExitCode(1062);
        public static TopshelfExitCode ServiceControlRequestFailed { get; } = new TopshelfExitCode(1064);
        public static TopshelfExitCode AbnormalExit { get; } = new TopshelfExitCode(1067);
        
        // non-compliant
        public static TopshelfExitCode SudoRequired { get; } = new TopshelfExitCode(2);
        public static TopshelfExitCode NotRunningOnWindows { get; } = new TopshelfExitCode(11);

        public override string ToString()
        {
            return $"Exit code: {_exitCode}";
        }
    }
}