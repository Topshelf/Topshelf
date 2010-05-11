namespace Topshelf
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ProcessExtensions
    {
        public static Process GetParent(this Process child)
        {
            int parentPid = 0;

            IntPtr hnd = Kernel32.CreateToolhelp32Snapshot(Kernel32.TH32CS_SNAPPROCESS, 0);

            if (hnd == IntPtr.Zero)
                return null;

            Kernel32.PROCESSENTRY32 processInfo = new Kernel32.PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(Kernel32.PROCESSENTRY32)) };

            if (Kernel32.Process32First(hnd, ref processInfo) == false) return null;

            do
            {
                if (child.Id == processInfo.th32ProcessID)
                    parentPid = (int)processInfo.th32ParentProcessID;
            }
            while (parentPid == 0 && Kernel32.Process32Next(hnd, ref processInfo));

            if (parentPid > 0)
                return Process.GetProcessById(parentPid);
            else
                return null;
        }
    }
}