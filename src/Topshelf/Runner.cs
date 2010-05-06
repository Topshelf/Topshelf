// Copyright 2007-2008 The Apache Software Foundation.
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
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Configuration;
    using log4net;
    using Model;

    /// <summary>
    /// Entry point into the Host infrastructure
    /// </summary>
    public static class Runner
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(Runner));

        static Runner()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Fatal("Host encountered an unhandled exception on the AppDomain", (Exception) e.ExceptionObject);
        }

        /// <summary>
        /// Go go gadget
        /// </summary>
        public static void Host(RunConfiguration configuration, string[] args)
        {
            _log.Info("Starting Host");
            if (args.Length > 0)
                _log.DebugFormat("Arguments: {0}", args);

            //make it so this can be passed in
            var argv = args.Aggregate("", (l, r) => "{0} {1}".FormatWith(l, r));
            var a = TopshelfArgumentParser.Parse(argv);
            TopshelfDispatcher.Dispatch(configuration, a);
        }
    }

    internal class ServiceControllerFactory : IServiceControllerFactory
    {
        public IServiceController CreateController()
        {
            if (Process.GetCurrentProcess().GetParent().ProcessName == "services")
                return new ScmServiceController();
            return new AsynchronousServiceController();
        }
    }
    public interface IServiceControllerFactory
    {
        IServiceController CreateController();
    }

    public class ScmServiceController :
        IServiceController
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Type ServiceType
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ServiceState State
        {
            get { throw new NotImplementedException(); }
        }

        public ServiceBuilder BuildService
        {
            get { throw new NotImplementedException(); }
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Continue()
        {
            throw new NotImplementedException();
        }
    }

    public class AsynchronousServiceController :
        IServiceController
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Type ServiceType
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ServiceState State
        {
            get { throw new NotImplementedException(); }
        }

        public ServiceBuilder BuildService
        {
            get { throw new NotImplementedException(); }
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Continue()
        {
            throw new NotImplementedException();
        }
    }
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

    internal static class Kernel32
    {
        public static uint TH32CS_SNAPPROCESS = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
    }
}