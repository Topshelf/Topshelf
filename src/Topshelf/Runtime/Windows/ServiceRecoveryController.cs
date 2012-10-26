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
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public class ServiceRecoveryController
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public void SetServiceRecoveryOptions(HostSettings settings, ServiceRecoveryOptions options)
        {
            IntPtr scmHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;
            IntPtr lpsaActions = IntPtr.Zero;
            IntPtr lpInfo = IntPtr.Zero;
            try
            {
                List<NativeMethods.SC_ACTION> actions = options.Actions.Select(x => x.GetAction()).ToList();
                if (actions.Count == 0)
                    throw new TopshelfException("Must be at least one failure action configured");

                scmHandle = NativeMethods.OpenSCManager(null, null, (int)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (scmHandle == IntPtr.Zero)
                    throw new TopshelfException("Failed to open service control manager");

                serviceHandle = NativeMethods.OpenService(scmHandle, settings.ServiceName,
                    (int)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (serviceHandle == IntPtr.Zero)
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

                if (!NativeMethods.ChangeServiceConfig2(serviceHandle,
                    NativeMethods.SERVICE_CONFIG_FAILURE_ACTIONS, lpInfo))
                {
                    throw new TopshelfException("Failed to change service recovery options");
                }
            }
            finally
            {
                if (lpInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpInfo);
                if (lpsaActions != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpsaActions);
                if (serviceHandle != IntPtr.Zero)
                    NativeMethods.CloseServiceHandle(serviceHandle);
                if (scmHandle != IntPtr.Zero)
                    NativeMethods.CloseServiceHandle(scmHandle);
            }
        }
    }
}