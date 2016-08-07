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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public class WindowsServiceRecoveryController
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public void SetServiceRecoveryOptions(HostSettings settings, ServiceRecoveryOptions options)
        {
            SafeTokenHandle scmHandle = null;
            SafeTokenHandle serviceHandle = null;
            IntPtr lpsaActions = IntPtr.Zero;
            IntPtr lpInfo = IntPtr.Zero;
            IntPtr lpFlagInfo = IntPtr.Zero;

            try
            {
                List<NativeMethods.SC_ACTION> actions = options.Actions.Select(x => x.GetAction()).ToList();
                if (actions.Count == 0)
                    throw new TopshelfException("Must be at least one failure action configured");

                scmHandle = NativeMethods.OpenSCManager(null, null, (int)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (scmHandle == null)
                    throw new TopshelfException("Failed to open service control manager");

                serviceHandle = NativeMethods.OpenService(scmHandle, settings.ServiceName,
                    (int)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (serviceHandle == null)
                    throw new TopshelfException("Failed to open service: " + settings.ServiceName);

                int actionSize = Marshal.SizeOf(typeof(NativeMethods.SC_ACTION));
                lpsaActions = Marshal.AllocHGlobal(actionSize*actions.Count + 1);
                if (lpsaActions == IntPtr.Zero)
                    throw new TopshelfException("Unable to allocate memory for service recovery actions");

                IntPtr nextAction = lpsaActions;
                for (int i = 0; i < actions.Count; i++)
                {
                    Marshal.StructureToPtr(actions[i], nextAction, false);
                    nextAction = (IntPtr)(nextAction.ToInt64() + actionSize);
                }

                var finalAction = new NativeMethods.SC_ACTION();
                finalAction.Type = (int)NativeMethods.SC_ACTION_TYPE.None;
                finalAction.Delay = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

                Marshal.StructureToPtr(finalAction, nextAction, false);

                string rebootMessage = options.Actions.Where(x => x.GetType() == typeof(RestartSystemRecoveryAction))
                                           .OfType<RestartSystemRecoveryAction>().Select(x => x.RestartMessage).
                                           FirstOrDefault() ?? "";

                string runProgramCommand = options.Actions.Where(x => x.GetType() == typeof(RunProgramRecoveryAction))
                                               .OfType<RunProgramRecoveryAction>().Select(x => x.Command).
                                               FirstOrDefault() ?? "";


                var failureActions = new NativeMethods.SERVICE_FAILURE_ACTIONS();
                failureActions.dwResetPeriod =
                    (int)TimeSpan.FromDays(options.ResetPeriod).TotalSeconds;
                failureActions.lpRebootMsg = rebootMessage;
                failureActions.lpCommand = runProgramCommand;
                failureActions.cActions = actions.Count + 1;
                failureActions.actions = lpsaActions;

                lpInfo = Marshal.AllocHGlobal(Marshal.SizeOf(failureActions));
                if (lpInfo == IntPtr.Zero)
                    throw new TopshelfException("Failed to allocate memory for failure actions");

                Marshal.StructureToPtr(failureActions, lpInfo, false);

                // If user specified a Restart option, get shutdown privileges
                if(options.Actions.Any(x => x.GetType() == typeof(RestartSystemRecoveryAction)))
                    RequestShutdownPrivileges();

                if (!NativeMethods.ChangeServiceConfig2(serviceHandle,
                    NativeMethods.SERVICE_CONFIG_FAILURE_ACTIONS, lpInfo))
                {
                    throw new TopshelfException(string.Format("Failed to change service recovery options. Windows Error: {0}", new Win32Exception().Message));
                }

                if (false == options.RecoverOnCrashOnly)
                {
                    var flag = new NativeMethods.SERVICE_FAILURE_ACTIONS_FLAG();
                    flag.fFailureActionsOnNonCrashFailures = true;

                    lpFlagInfo = Marshal.AllocHGlobal(Marshal.SizeOf(flag));
                    if (lpFlagInfo == IntPtr.Zero)
                        throw new TopshelfException("Failed to allocate memory for failure flag");

                    Marshal.StructureToPtr(flag, lpFlagInfo, false);

                    try
                    {
                        NativeMethods.ChangeServiceConfig2(serviceHandle,
                            NativeMethods.SERVICE_CONFIG_FAILURE_ACTIONS_FLAG, lpFlagInfo);
                    }
                    catch
                    {
                        // this fails on XP, but we don't care really as it's optional
                    }
                }
            }
            finally
            {
                if (lpFlagInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpFlagInfo);
                if (lpInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpInfo);
                if (lpsaActions != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpsaActions);
                if (serviceHandle != null)
                    NativeMethods.CloseServiceHandle(serviceHandle);
                if (scmHandle != null)
                    NativeMethods.CloseServiceHandle(scmHandle);
            }
        }

        private void RequestShutdownPrivileges()
        {
            SafeTokenHandle hToken;
            ThrowOnFail(
                NativeMethods.OpenProcessToken(System.Diagnostics.Process.GetCurrentProcess().Handle,
                (int)NativeMethods.SYSTEM_ACCESS.TOKEN_ADJUST_PRIVILEGES | 
                (int)NativeMethods.SYSTEM_ACCESS.TOKEN_QUERY, out hToken));

            NativeMethods.TOKEN_PRIVILEGES tkp;
            tkp.PrivilegeCount = 1;
            tkp.Privileges.Attributes = (int)NativeMethods.SYSTEM_ACCESS.SE_PRIVILEGE_ENABLED;
            const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
            ThrowOnFail(
                NativeMethods.LookupPrivilegeValue("", SE_SHUTDOWN_NAME, out tkp.Privileges.pLuid));

            ThrowOnFail(
                NativeMethods.AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero));
        }

        private static void ThrowOnFail(bool success)
        {
            if(!success)
                throw new TopshelfException(string.Format(
                    "Computer shutdown was specified as a recovery option, but privileges could not be acquired. Windows Error: {0}",
                    new Win32Exception().Message));
        }
    }
}