using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.ServiceProcess
{
    public partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class LSA_UNICODE_STRING_withPointer
        {
            public short length;

            public short maximumLength;

            public IntPtr pwstr = (IntPtr)0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class LSA_UNICODE_STRING
        {
            public short length;

            public short maximumLength;

            public string buffer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class LSA_OBJECT_ATTRIBUTES
        {
            public int length;

            public IntPtr rootDirectory = (IntPtr)0;

            public IntPtr pointerLsaString = (IntPtr)0;

            public int attributes;

            public IntPtr pointerSecurityDescriptor = (IntPtr)0;

            public IntPtr pointerSecurityQualityOfService = (IntPtr)0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SERVICE_DELAYED_AUTOSTART_INFO
        {
            public bool fDelayedAutostart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SERVICE_DESCRIPTION
        {
            public IntPtr description;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr serviceHandle, uint infoLevel, ref SERVICE_DESCRIPTION serviceDesc);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr serviceHandle, uint infoLevel, ref SERVICE_DELAYED_AUTOSTART_INFO serviceDesc);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateService(IntPtr databaseHandle, string serviceName, string displayName, int access, int serviceType, int startType, int errorControl, string binaryPath, string loadOrderGroup, IntPtr pTagId, string dependencies, string servicesStartName, string password);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool LookupAccountName(string systemName, string accountName, byte[] sid, int[] sidLen, char[] refDomainName, int[] domNameLen, [In] [Out] int[] sidNameUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int LsaAddAccountRights(IntPtr policyHandle, byte[] accountSid, LSA_UNICODE_STRING userRights, int countOfRights);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int LsaEnumerateAccountRights(IntPtr policyHandle, byte[] accountSid, out IntPtr pLsaUnicodeStringUserRights, out int RightsCount);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int LsaOpenPolicy(LSA_UNICODE_STRING systemName, IntPtr pointerObjectAttributes, int desiredAccess, out IntPtr pointerPolicyHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int LsaRemoveAccountRights(IntPtr policyHandle, byte[] accountSid, bool allRights, LSA_UNICODE_STRING userRights, int countOfRights);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetComputerName(StringBuilder lpBuffer, ref int nSize);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool DeleteService(IntPtr serviceHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr databaseHandle, string serviceName, int access);

    }
}
