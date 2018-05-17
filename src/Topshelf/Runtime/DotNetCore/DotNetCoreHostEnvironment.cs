// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Runtime.DotNetCore
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceProcess;
    using Windows;
    using HostConfigurators;
    using Logging;


    public class DotNetCoreHostEnvironment :
        HostEnvironment
    {
        readonly LogWriter _log = HostLogger.Get(typeof(DotNetCoreHostEnvironment));
        readonly HostConfigurator _hostConfigurator;

        public DotNetCoreHostEnvironment(HostConfigurator configurator)
        {
            _hostConfigurator = configurator;
        }

        public string CommandLine => CommandLineParser.CommandLine.GetUnparsedCommandLine();

        public bool IsAdministrator
        {
            get
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return false;

                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                var principal = new WindowsPrincipal(identity);

                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public bool IsRunningAsAService
        {
            get
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return false;

                try
                {
                    Process process = GetParent(Process.GetCurrentProcess());
                    if (process != null && process.ProcessName == "services")
                    {
                        _log.Debug("Started by the Windows services process");
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // again, mono seems to fail with this, let's just return false okay?
                }

                return false;
            }
        }

        public bool IsServiceInstalled(string serviceName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            return IsServiceListed(serviceName);
        }

        public bool IsServiceStopped(string serviceName)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return true;

            using (var sc = new ServiceController(serviceName))
            {
                return sc.Status == ServiceControllerStatus.Stopped;
            }
        }

        public void StartService(string serviceName, TimeSpan startTimeOut)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    _log.InfoFormat("The {0} service is already running.", serviceName);
                    return;
                }

                if (sc.Status == ServiceControllerStatus.StartPending)
                {
                    _log.InfoFormat("The {0} service is already starting.", serviceName);
                    return;
                }

                if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.Paused)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, startTimeOut);
                }
                else
                {
                    // Status is StopPending, ContinuePending or PausedPending, print warning
                    _log.WarnFormat("The {0} service can't be started now as it has the status {1}. Try again later...", serviceName, sc.Status.ToString());
                }
            }
        }

        public void StopService(string serviceName, TimeSpan stopTimeOut)
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

                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, stopTimeOut);
                }
                else
                {
                    // Status is StartPending, ContinuePending or PausedPending, print warning
                    _log.WarnFormat("The {0} service can't be stopped now as it has the status {1}. Try again later...", serviceName, sc.Status.ToString());
                }
            }
        }

#if !NETCORE
        public void InstallService(InstallHostSettings settings, Action<InstallHostSettings> beforeInstall, Action afterInstall, Action beforeRollback,
            Action afterRollback)
        {
            throw new NotImplementedException();
        }
#endif

        public void UninstallService(HostSettings settings, Action beforeUninstall, Action afterUninstall)
        {
            throw new NotImplementedException();
        }

        public bool RunAsAdministrator()
        {
            throw new NotImplementedException();
        }

        public Host CreateServiceHost(HostSettings settings, ServiceHandle serviceHandle)
        {
            return new WindowsServiceHost(this, settings, serviceHandle, this._hostConfigurator);
        }

        public void SendServiceCommand(string serviceName, int command)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.ExecuteCommand(command);
                }
                else
                {
                    _log.WarnFormat("The {0} service can't be commanded now as it has the status {1}. Try again later...",
                        serviceName, sc.Status.ToString());
                }
            }
        }

        Process GetParent(Process child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            try
            {
                int parentPid = 0;

                IntPtr hnd = Kernel32.CreateToolhelp32Snapshot(Kernel32.TH32CS_SNAPPROCESS, 0);

                if (hnd == IntPtr.Zero)
                    return null;

                var processInfo = new Kernel32.PROCESSENTRY32
                {
                    dwSize = (uint) Marshal.SizeOf(typeof(Kernel32.PROCESSENTRY32))
                };

                if (Kernel32.Process32First(hnd, ref processInfo) == false)
                    return null;

                do
                {
                    if (child.Id == processInfo.th32ProcessID)
                        parentPid = (int) processInfo.th32ParentProcessID;
                } while (parentPid == 0 && Kernel32.Process32Next(hnd, ref processInfo));

                if (parentPid > 0)
                    return Process.GetProcessById(parentPid);
            }
            catch (Exception ex)
            {
                _log.Error("Unable to get parent process (ignored)", ex);
            }

            return null;
        }

        bool IsServiceListed(string serviceName)
        {
            bool result = false;

            try
            {
                result = ServiceController.GetServices()
                    .Any(service => string.Equals(service.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidOperationException)
            {
                _log.Debug("Cannot access Service List due to permissions. Assuming the service is not installed.");
            }

            return result;
        }
    }
}