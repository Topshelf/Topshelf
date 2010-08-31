﻿// Copyright 2007-2010 The Apache Software Foundation.
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
    using System.Runtime.InteropServices;
    using WindowsServiceCode;

    public static class ProcessExtensions
    {
        public static Process GetParent(this Process child)
        {
            int parentPid = 0;

            IntPtr hnd = Kernel32.CreateToolhelp32Snapshot(Kernel32.TH32CS_SNAPPROCESS, 0);

            if (hnd == IntPtr.Zero)
                return null;

            var processInfo = new Kernel32.PROCESSENTRY32
                                  {dwSize = (uint) Marshal.SizeOf(typeof (Kernel32.PROCESSENTRY32))};

            if (Kernel32.Process32First(hnd, ref processInfo) == false) return null;

            do
            {
                if (child.Id == processInfo.th32ProcessID)
                    parentPid = (int) processInfo.th32ParentProcessID;
            } while (parentPid == 0 && Kernel32.Process32Next(hnd, ref processInfo));

            if (parentPid > 0)
                return Process.GetProcessById(parentPid);
            else
                return null;
        }
    }
}