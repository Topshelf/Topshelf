namespace Topshelf.Shelving
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class WellknownAddresses
    {
        public static Uri CurrentShelfAddress
        {
            get
            {
                return new Uri("pipes://localhost/topshelf-{0}-{1}".FormatWith(GetFolder(), GetPid()));
            }
        }

        public static Uri HostAddress
        {
            get
            {
                return new Uri("pipes://localhost/topshelf-host-{0}".FormatWith(GetPid()));
            }
        }

        public static int GetPid()
        {
            return Process.GetCurrentProcess().Id;
        }

        public static string GetFolder()
        {
            return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
        }
    }
}