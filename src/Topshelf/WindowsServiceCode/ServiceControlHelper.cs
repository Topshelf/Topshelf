using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.ServiceProcess;
using Microsoft.Win32;

namespace Topshelf.WindowsServiceCode
{
  public class ServiceControlHelper
  {
    #region "SERVICE RECOVERY INTEROP"
    // ReSharper disable InconsistentNaming
    enum SC_ACTION_TYPE
    {
      None = 0,
      RestartService = 1,
      RebootComputer = 2,
      RunCommand = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SC_ACTION
    {
      public SC_ACTION_TYPE Type;
      public uint Delay;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct SERVICE_FAILURE_ACTIONS
    {
      public int dwResetPeriod;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string lpRebootMsg;
      [MarshalAs(UnmanagedType.LPWStr)]
      public string lpCommand;
      public int cActions;
      public IntPtr lpsaActions;
    }

    private const int SERVICE_CONFIG_FAILURE_ACTIONS = 2;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ChangeServiceConfig2(
        IntPtr hService,
        int dwInfoLevel,
        IntPtr lpInfo);

    [DllImport("advapi32.dll", EntryPoint = "QueryServiceConfig2", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int QueryServiceConfig2(
        IntPtr hService,
        int dwInfoLevel,
        IntPtr lpBuffer,
        uint cbBufSize,
        out uint pcbBytesNeeded);

    // ReSharper restore InconsistentNaming
    #endregion

    #region "GRANT SHUTDOWN INTEROP"
    // ReSharper disable InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    struct LUID_AND_ATTRIBUTES
    {
      public long Luid;
      public UInt32 Attributes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TOKEN_PRIVILEGES
    {
      public int PrivilegeCount;
      public LUID_AND_ATTRIBUTES Privileges;

    }

    [DllImport("advapi32.dll")]
    private static extern bool
        AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
        [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES NewState, int BufferLength,
       IntPtr PreviousState, ref int ReturnLength);


    [DllImport("advapi32.dll")]
    private static extern bool
        LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

    [DllImport("advapi32.dll")]
    private static extern bool
        OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

    private const int TOKEN_ADJUST_PRIVILEGES = 32;
    private const int TOKEN_QUERY = 8;
    private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
    private const int SE_PRIVILEGE_ENABLED = 2;
    // ReSharper restore InconsistentNaming
    #endregion

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public static void SetServiceRecoveryOptions(
        string serviceName,
        ServiceRecoveryOptions recoveryOptions)
    {
      //ThrowHelper.ThrowArgumentNullIfNull(serviceName, "serviceName");
      //ThrowHelper.ThrowArgumentOutOfRangeIfEmpty(serviceName, "serviceName");

      //log.Debug(m => m("Setting service recovery options..."));

      bool requiresShutdownPriveleges =
          recoveryOptions.FirstFailureAction == ServiceRecoveryAction.RestartTheComputer ||
          recoveryOptions.SecondFailureAction == ServiceRecoveryAction.RestartTheComputer ||
          recoveryOptions.SubsequentFailureActions == ServiceRecoveryAction.RestartTheComputer;
      if (requiresShutdownPriveleges)
      {
        GrantShutdownPrivileges();
      }

      int actionCount = 3;
      var restartServiceAfter = (uint)TimeSpan.FromMinutes(
          recoveryOptions.MinutesToRestartService).TotalMilliseconds;

      IntPtr failureActionsPointer = IntPtr.Zero;
      IntPtr actionPointer = IntPtr.Zero;

      ServiceController controller = null;
      try
      {
        // Open the service
        controller = new ServiceController(serviceName);

        // Set up the failure actions
        var failureActions = new SERVICE_FAILURE_ACTIONS();
        failureActions.dwResetPeriod = (int)TimeSpan.FromDays(recoveryOptions.DaysToResetFailAcount).TotalSeconds;
        failureActions.cActions = actionCount;
        failureActions.lpRebootMsg = recoveryOptions.RebootMessage;

        // allocate memory for the individual actions
        actionPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SC_ACTION)) * actionCount);
        ServiceRecoveryAction[] actions = { recoveryOptions.FirstFailureAction,
                                                    recoveryOptions.SecondFailureAction,
                                                    recoveryOptions.SubsequentFailureActions };
        for (int i = 0; i < actions.Length; i++)
        {
          ServiceRecoveryAction action = actions[i];
          var scAction = GetScAction(action, restartServiceAfter);
          Marshal.StructureToPtr(scAction, (IntPtr)((Int64)actionPointer + (Marshal.SizeOf(typeof(SC_ACTION))) * i), false);
        }
        failureActions.lpsaActions = actionPointer;

        string command = recoveryOptions.CommandToLaunchOnFailure;
        if (command != null)
        {
          failureActions.lpCommand = command;
        }

        failureActionsPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SERVICE_FAILURE_ACTIONS)));
        Marshal.StructureToPtr(failureActions, failureActionsPointer, false);

        // Make the change
        bool success = ChangeServiceConfig2(
            controller.ServiceHandle.DangerousGetHandle(),
            SERVICE_CONFIG_FAILURE_ACTIONS,
            failureActionsPointer);

        // Check that the change occurred
        if (!success)
        {
          throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to change the Service configuration.");
        }
      }
      finally
      {
        if (failureActionsPointer != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(failureActionsPointer);
        }

        if (actionPointer != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(actionPointer);
        }

        if (controller != null)
        {
          controller.Close();
        }

        //log.Debug(m => m("Done setting service recovery options."));
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private static void GrantShutdownPrivileges()
    {
      //log.Debug(m => m("Granting shutdown privileges to process user..."));

      IntPtr tokenHandle = IntPtr.Zero;

      TOKEN_PRIVILEGES tkp = new TOKEN_PRIVILEGES();

      long luid = 0;
      int retLen = 0;

      try
      {
        IntPtr processHandle = Process.GetCurrentProcess().Handle;
        bool success = OpenProcessToken(processHandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref tokenHandle);
        if (!success)
        {
          throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open process token.");
        }

        LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref luid);

        tkp.PrivilegeCount = 1;
        tkp.Privileges.Luid = luid;
        tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

        success = AdjustTokenPrivileges(tokenHandle, false, ref tkp, 0, IntPtr.Zero, ref retLen);
        if (!success)
        {
          throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to shutdown priveleges.");
        }
      }
      finally
      {
        if (tokenHandle != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(tokenHandle);
        }
        //log.Debug(m => m("Done granting shutdown privileges to process user."));
      }
    }

    private static SC_ACTION GetScAction(ServiceRecoveryAction action,
        uint restartServiceAfter)
    {
      var scAction = new SC_ACTION();
      SC_ACTION_TYPE actionType = default(SC_ACTION_TYPE);
      switch (action)
      {
        case ServiceRecoveryAction.TakeNoAction:
          actionType = SC_ACTION_TYPE.None;
          break;
        case ServiceRecoveryAction.RestartTheService:
          actionType = SC_ACTION_TYPE.RestartService;
          break;
        case ServiceRecoveryAction.RestartTheComputer:
          actionType = SC_ACTION_TYPE.RebootComputer;
          break;
        case ServiceRecoveryAction.RunAProgram:
          actionType = SC_ACTION_TYPE.RunCommand;
          break;
      }
      scAction.Type = actionType;
      scAction.Delay = restartServiceAfter;
      return scAction;
    }
  }
}
