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
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Security.Permissions;
	using System.ServiceProcess;
	using System.Threading;
	using log4net;
	using Magnum.Extensions;


	/// <summary>
	/// Taken from http://code.google.com/p/daemoniq. Thanks guys!
	/// </summary>
	public static class WindowsServiceControlManager
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Windows.WindowsServiceControlManager");

		const int SERVICE_CONFIG_FAILURE_ACTIONS = 2;
		const int SE_PRIVILEGE_ENABLED = 2;
		const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
		const int TOKEN_ADJUST_PRIVILEGES = 32;
		const int TOKEN_QUERY = 8;

		public static bool IsInstalled(string serviceName)
		{
			return ServiceController
				.GetServices()
				.Any(service => service.ServiceName == serviceName);
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ChangeServiceConfig2(
			IntPtr hService,
			int dwInfoLevel,
			IntPtr lpInfo);

		[DllImport("advapi32.dll", EntryPoint = "QueryServiceConfig2", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern int QueryServiceConfig2(
			IntPtr hService,
			int dwInfoLevel,
			IntPtr lpBuffer,
			uint cbBufSize,
			out uint pcbBytesNeeded);


		[DllImport("advapi32.dll")]
		static extern bool
			AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
			                      [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength,
			                      IntPtr PreviousState, ref int ReturnLength);


		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		static extern bool
			LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

		[DllImport("advapi32.dll")]
		static extern bool
			OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static void SetServiceRecoveryOptions(
			string serviceName,
			ServiceRecoveryOptions recoveryOptions)
		{
			bool requiresShutdownPriveleges =
				recoveryOptions.FirstFailureAction == ServiceRecoveryAction.RestartComputer ||
				recoveryOptions.SecondFailureAction == ServiceRecoveryAction.RestartComputer ||
				recoveryOptions.SubsequentFailureAction == ServiceRecoveryAction.RestartComputer;
			if (requiresShutdownPriveleges)
				GrantShutdownPrivileges();

			int actionCount = 3;
			var restartServiceAfter = (uint)TimeSpan.FromMinutes(
			                                                     recoveryOptions.RestartServiceWaitMinutes).TotalMilliseconds;

			IntPtr failureActionsPointer = IntPtr.Zero;
			IntPtr actionPointer = IntPtr.Zero;

			ServiceController controller = null;
			try
			{
				// Open the service
				controller = new ServiceController(serviceName);

				// Set up the failure actions
				var failureActions = new SERVICE_FAILURE_ACTIONS();
				failureActions.dwResetPeriod = (int)TimeSpan.FromDays(recoveryOptions.ResetFailureCountWaitDays).TotalSeconds;
				failureActions.cActions = (uint)actionCount;
				failureActions.lpRebootMsg = recoveryOptions.RestartSystemMessage;

				// allocate memory for the individual actions
				actionPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SC_ACTION))*actionCount);
				ServiceRecoveryAction[] actions = {
				                                  	recoveryOptions.FirstFailureAction,
				                                  	recoveryOptions.SecondFailureAction,
				                                  	recoveryOptions.SubsequentFailureAction
				                                  };
				for (int i = 0; i < actions.Length; i++)
				{
					ServiceRecoveryAction action = actions[i];
					SC_ACTION scAction = GetScAction(action, restartServiceAfter);
					Marshal.StructureToPtr(scAction, (IntPtr)((Int64)actionPointer + (Marshal.SizeOf(typeof(SC_ACTION)))*i), false);
				}
				failureActions.lpsaActions = actionPointer;

				string command = recoveryOptions.RunProgramCommand;
				if (command != null)
					failureActions.lpCommand = command;

				failureActionsPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SERVICE_FAILURE_ACTIONS)));
				Marshal.StructureToPtr(failureActions, failureActionsPointer, false);

				// Make the change
				bool success = ChangeServiceConfig2(controller.ServiceHandle.DangerousGetHandle(),
				                                    SERVICE_CONFIG_FAILURE_ACTIONS,
				                                    failureActionsPointer);

				// Check that the change occurred
				if (!success)
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to change the Service configuration.");
			}
			finally
			{
				if (failureActionsPointer != IntPtr.Zero)
					Marshal.FreeHGlobal(failureActionsPointer);

				if (actionPointer != IntPtr.Zero)
					Marshal.FreeHGlobal(actionPointer);

				if (controller != null)
				{
					controller.Dispose();
				}

				//log.Debug(m => m("Done setting service recovery options."));
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		static void GrantShutdownPrivileges()
		{
			//log.Debug(m => m("Granting shutdown privileges to process user..."));

			IntPtr tokenHandle = IntPtr.Zero;

			var tkp = new TOKEN_PRIVILEGES();

			long luid = 0;
			int retLen = 0;

			try
			{
				IntPtr processHandle = Process.GetCurrentProcess().Handle;
				bool success = OpenProcessToken(processHandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref tokenHandle);
				if (!success)
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open process token.");

				LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref luid);

				tkp.PrivilegeCount = 1;
				tkp.Privileges.Luid = luid;
				tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

				success = AdjustTokenPrivileges(tokenHandle, false, ref tkp, 0, IntPtr.Zero, ref retLen);
				if (!success)
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to shutdown priveleges.");
			}
			finally
			{
				if (tokenHandle != IntPtr.Zero)
					Marshal.FreeHGlobal(tokenHandle);
				//log.Debug(m => m("Done granting shutdown privileges to process user."));
			}
		}

		static SC_ACTION GetScAction(ServiceRecoveryAction action,
		                             uint restartServiceAfter)
		{
			var scAction = new SC_ACTION();
			SC_ACTION_TYPE actionType = default(SC_ACTION_TYPE);
			switch (action)
			{
				case ServiceRecoveryAction.TakeNoAction:
					actionType = SC_ACTION_TYPE.None;
					break;
				case ServiceRecoveryAction.RestartService:
					actionType = SC_ACTION_TYPE.RestartService;
					break;
				case ServiceRecoveryAction.RestartComputer:
					actionType = SC_ACTION_TYPE.RebootComputer;
					break;
				case ServiceRecoveryAction.RunProgram:
					actionType = SC_ACTION_TYPE.RunCommand;
					break;
			}
			scAction.Type = actionType;
			scAction.Delay = restartServiceAfter;
			return scAction;
		}


		[StructLayout(LayoutKind.Sequential)]
		struct LUID_AND_ATTRIBUTES
		{
			[MarshalAs(UnmanagedType.U4)]
			public UInt32 Attributes;
			public long Luid;
		}


		[StructLayout(LayoutKind.Sequential)]
		struct SC_ACTION
		{
			[MarshalAs(UnmanagedType.U4)]
			public uint Delay;
			[MarshalAs(UnmanagedType.U4)]
			public SC_ACTION_TYPE Type;
		}


		enum SC_ACTION_TYPE
		{
			None = 0,
			RestartService = 1,
			RebootComputer = 2,
			RunCommand = 3
		}


		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct SERVICE_FAILURE_ACTIONS
		{
			[MarshalAs(UnmanagedType.U4)]
			public int dwResetPeriod;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpRebootMsg;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpCommand;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 cActions;

			public IntPtr lpsaActions;
		}


		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct TOKEN_PRIVILEGES
		{
			public int PrivilegeCount;
			public LUID_AND_ATTRIBUTES Privileges;
		}


		public static void Start(string serviceName)
		{
			using (var sc = new ServiceController(serviceName))
			{
				if(sc.Status == ServiceControllerStatus.Running)
				{
					_log.InfoFormat("The {0} service is already running.", serviceName);
					return;
				}

				if(sc.Status == ServiceControllerStatus.StartPending)
				{
					_log.InfoFormat("The {0} service is already starting.", serviceName);
					return;
				}

				sc.Start();
				while (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StartPending)
				{
					Thread.Sleep(1000);
					sc.Refresh();
				}
			}
		}

		public static void Stop(string serviceName)
		{
			using (var sc = new ServiceController(serviceName))
			{
				if (sc.Status == ServiceControllerStatus.Stopped)
				{
					_log.InfoFormat("The {0} service is not running.", serviceName);
					return;
				}

				if (sc.Status == ServiceControllerStatus.StopPending)
				{
					_log.InfoFormat("The {0} service is already stopping.", serviceName);
					return;
				}

				sc.Stop();
				while (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StopPending	)
				{
					Thread.Sleep(1000);
					sc.Refresh();
				}
			}
		}
	}
}