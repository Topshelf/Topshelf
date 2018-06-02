using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace System.ServiceProcess
{
    [SuppressUnmanagedCodeSecurity]
    public partial class SafeNativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int LsaClose(IntPtr objectHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int LsaNtStatusToWinError(int ntStatus);

        [DllImport("advapi32.dll")]
        public static extern int LsaFreeMemory(IntPtr ptr);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, int access);

    }
}
