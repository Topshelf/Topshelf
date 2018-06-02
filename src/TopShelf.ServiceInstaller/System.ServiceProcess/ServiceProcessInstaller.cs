using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.ServiceProcess
{
    public class ServiceProcessInstaller : ComponentInstaller
    {
        private ServiceAccount serviceAccount = ServiceAccount.User;

        private bool haveLoginInfo;

        private string password;

        private string username;

        private static bool helpPrinted;

        /// <summary>Gets help text displayed for service installation options.</summary>
        /// <returns>Help text that provides a description of the steps for setting the user name and password in order to run the service under a particular account.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        public override string HelpText
        {
            get
            {
                if (ServiceProcessInstaller.helpPrinted)
                {
                    return base.HelpText;
                }
                ServiceProcessInstaller.helpPrinted = true;
                return Res.GetString("HelpText") + "\r\n" + base.HelpText;
            }
        }

        /// <summary>Gets or sets the password associated with the user account under which the service application runs.</summary>
        /// <returns>The password associated with the account under which the service should run. The default is an empty string (""). The property is not public, and is never serialized.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        [Browsable(false)]
        public string Password
        {
            get
            {
                if (!this.haveLoginInfo)
                {
                    this.GetLoginInfo();
                }
                return this.password;
            }
            set
            {
                this.haveLoginInfo = false;
                this.password = value;
            }
        }

        /// <summary>Gets or sets the type of account under which to run this service application.</summary>
        /// <returns>A <see cref="T:System.ServiceProcess.ServiceAccount" /> that defines the type of account under which the system runs this service. The default is User.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        [DefaultValue(ServiceAccount.User)]
        [ServiceProcessDescription("ServiceProcessInstallerAccount")]
        public ServiceAccount Account
        {
            get
            {
                if (!this.haveLoginInfo)
                {
                    this.GetLoginInfo();
                }
                return this.serviceAccount;
            }
            set
            {
                this.haveLoginInfo = false;
                this.serviceAccount = value;
            }
        }

        /// <summary>Gets or sets the user account under which the service application will run.</summary>
        /// <returns>The account under which the service should run. The default is an empty string ("").</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Browsable(false)]
        public string Username
        {
            get
            {
                if (!this.haveLoginInfo)
                {
                    this.GetLoginInfo();
                }
                return this.username;
            }
            set
            {
                this.haveLoginInfo = false;
                this.username = value;
            }
        }

        private static bool AccountHasRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            IntPtr intPtr = (IntPtr)0;
            int num = 0;
            int num2 = NativeMethods.LsaEnumerateAccountRights(policyHandle, accountSid, out intPtr, out num);
            switch (num2)
            {
                case -1073741772:
                    return false;
                default:
                    throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num2));
                case 0:
                    {
                        bool result = false;
                        try
                        {
                            IntPtr intPtr2 = intPtr;
                            for (int i = 0; i < num; i++)
                            {
                                NativeMethods.LSA_UNICODE_STRING_withPointer lSA_UNICODE_STRING_withPointer = new NativeMethods.LSA_UNICODE_STRING_withPointer();
                                Marshal.PtrToStructure(intPtr2, (object)lSA_UNICODE_STRING_withPointer);
                                char[] array = new char[lSA_UNICODE_STRING_withPointer.length / 2];
                                Marshal.Copy(lSA_UNICODE_STRING_withPointer.pwstr, array, 0, array.Length);
                                if (string.Compare(new string(array, 0, array.Length), rightName, StringComparison.Ordinal) == 0)
                                {
                                    return true;
                                }
                                intPtr2 = (IntPtr)((long)intPtr2 + Marshal.SizeOf(typeof(NativeMethods.LSA_UNICODE_STRING)));
                            }
                            return result;
                        }
                        finally
                        {
                            SafeNativeMethods.LsaFreeMemory(intPtr);
                        }
                    }
            }
        }

        /// <summary>Implements the base class <see cref="M:System.Configuration.Install.ComponentInstaller.CopyFromComponent(System.ComponentModel.IComponent)" /> method with no <see cref="T:System.ServiceProcess.ServiceProcessInstaller" /> class-specific behavior.</summary>
        /// <param name="comp">The <see cref="T:System.ComponentModel.IComponent" /> that represents the service process. </param>
        public override void CopyFromComponent(IComponent comp)
        {
        }

        private byte[] GetAccountSid(string accountName)
        {
            byte[] array = new byte[256];
            int[] array2 = new int[1]
            {
            array.Length
            };
            char[] array3 = new char[1024];
            int[] domNameLen = new int[1]
            {
            array3.Length
            };
            int[] sidNameUse = new int[1];
            if (accountName.Substring(0, 2) == ".\\")
            {
                StringBuilder stringBuilder = new StringBuilder(32);
                int num = 32;
                if (!NativeMethods.GetComputerName(stringBuilder, ref num))
                {
                    throw new Win32Exception();
                }
                accountName = stringBuilder + accountName.Substring(1);
            }
            if (!NativeMethods.LookupAccountName(null, accountName, array, array2, array3, domNameLen, sidNameUse))
            {
                throw new Win32Exception();
            }
            byte[] array4 = new byte[array2[0]];
            Array.Copy(array, 0, array4, 0, array2[0]);
            return array4;
        }

        private void GetLoginInfo()
        {
            if (base.Context != null && !base.DesignMode && !this.haveLoginInfo)
            {
                this.haveLoginInfo = true;
                if (this.serviceAccount != ServiceAccount.User)
                {
                    return;
                }
                if (base.Context.Parameters.ContainsKey("username"))
                {
                    this.username = base.Context.Parameters["username"];
                }
                if (base.Context.Parameters.ContainsKey("password"))
                {
                    this.password = base.Context.Parameters["password"];
                }
                if (this.username != null && this.username.Length != 0 && this.password != null)
                {
                    return;
                }
                if (!base.Context.Parameters.ContainsKey("unattended"))
                {
                    throw new PlatformNotSupportedException();
                    //using (ServiceInstallerDialog serviceInstallerDialog = new ServiceInstallerDialog())
                    //{
                    //    if (this.username != null)
                    //    {
                    //        serviceInstallerDialog.Username = this.username;
                    //    }
                    //    serviceInstallerDialog.ShowDialog();
                    //    switch (serviceInstallerDialog.Result)
                    //    {
                    //        case ServiceInstallerDialogResult.Canceled:
                    //            throw new InvalidOperationException(Res.GetString("UserCanceledInstall", base.Context.Parameters["assemblypath"]));
                    //        case ServiceInstallerDialogResult.UseSystem:
                    //            this.username = null;
                    //            this.password = null;
                    //            this.serviceAccount = ServiceAccount.LocalSystem;
                    //            break;
                    //        case ServiceInstallerDialogResult.OK:
                    //            this.username = serviceInstallerDialog.Username;
                    //            this.password = serviceInstallerDialog.Password;
                    //            break;
                    //    }
                    //}
                    //return;
                }
                throw new InvalidOperationException(Res.GetString("UnattendedCannotPrompt", base.Context.Parameters["assemblypath"]));
            }
        }

        private static void GrantAccountRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING = new NativeMethods.LSA_UNICODE_STRING();
            lSA_UNICODE_STRING.buffer = rightName;
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING2 = lSA_UNICODE_STRING;
            lSA_UNICODE_STRING2.length = (short)(lSA_UNICODE_STRING2.buffer.Length * 2);
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING3 = lSA_UNICODE_STRING;
            lSA_UNICODE_STRING3.maximumLength = lSA_UNICODE_STRING3.length;
            int num = NativeMethods.LsaAddAccountRights(policyHandle, accountSid, lSA_UNICODE_STRING, 1);
            if (num == 0)
            {
                return;
            }
            throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num));
        }

        /// <summary>Writes service application information to the registry. This method is meant to be used by installation tools, which call the appropriate methods automatically.</summary>
        /// <param name="stateSaver">An <see cref="T:System.Collections.IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="T:System.ArgumentException">The <paramref name="stateSaver" /> is null. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        ///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override void Install(IDictionary stateSaver)
        {
            try
            {
                ServiceInstaller.CheckEnvironment();
                try
                {
                    if (!this.haveLoginInfo)
                    {
                        try
                        {
                            this.GetLoginInfo();
                        }
                        catch
                        {
                            stateSaver["hadServiceLogonRight"] = true;
                            throw;
                        }
                    }
                }
                finally
                {
                    stateSaver["Account"] = this.Account;
                    if (this.Account == ServiceAccount.User)
                    {
                        stateSaver["Username"] = this.Username;
                    }
                }
                if (this.Account == ServiceAccount.User)
                {
                    IntPtr intPtr = this.OpenSecurityPolicy();
                    bool flag = true;
                    try
                    {
                        byte[] accountSid = this.GetAccountSid(this.Username);
                        flag = ServiceProcessInstaller.AccountHasRight(intPtr, accountSid, "SeServiceLogonRight");
                        if (!flag)
                        {
                            ServiceProcessInstaller.GrantAccountRight(intPtr, accountSid, "SeServiceLogonRight");
                        }
                    }
                    finally
                    {
                        stateSaver["hadServiceLogonRight"] = flag;
                        SafeNativeMethods.LsaClose(intPtr);
                    }
                }
            }
            finally
            {
                base.Install(stateSaver);
            }
        }

        private IntPtr OpenSecurityPolicy()
        {
            GCHandle gCHandle = GCHandle.Alloc(new NativeMethods.LSA_OBJECT_ATTRIBUTES(), GCHandleType.Pinned);
            try
            {
                int num = 0;
                IntPtr pointerObjectAttributes = gCHandle.AddrOfPinnedObject();
                IntPtr result = default(IntPtr);
                num = NativeMethods.LsaOpenPolicy((NativeMethods.LSA_UNICODE_STRING)null, pointerObjectAttributes, 2064, out result);
                if (num != 0)
                {
                    throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num));
                }
                return result;
            }
            finally
            {
                gCHandle.Free();
            }
        }

        private static void RemoveAccountRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING = new NativeMethods.LSA_UNICODE_STRING();
            lSA_UNICODE_STRING.buffer = rightName;
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING2 = lSA_UNICODE_STRING;
            lSA_UNICODE_STRING2.length = (short)(lSA_UNICODE_STRING2.buffer.Length * 2);
            NativeMethods.LSA_UNICODE_STRING lSA_UNICODE_STRING3 = lSA_UNICODE_STRING;
            lSA_UNICODE_STRING3.maximumLength = lSA_UNICODE_STRING3.length;
            int num = NativeMethods.LsaRemoveAccountRights(policyHandle, accountSid, false, lSA_UNICODE_STRING, 1);
            if (num == 0)
            {
                return;
            }
            throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(num));
        }

        /// <summary>Rolls back service application information written to the registry by the installation procedure. This method is meant to be used by installation tools, which process the appropriate methods automatically.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the context information associated with the installation. </param>
        /// <exception cref="T:System.ArgumentException">The <paramref name="savedState" /> is null.-or- The <paramref name="savedState" /> is corrupted or non-existent. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        public override void Rollback(IDictionary savedState)
        {
            try
            {
                if ((ServiceAccount)savedState["Account"] == ServiceAccount.User && !(bool)savedState["hadServiceLogonRight"])
                {
                    string accountName = (string)savedState["Username"];
                    IntPtr intPtr = this.OpenSecurityPolicy();
                    try
                    {
                        byte[] accountSid = this.GetAccountSid(accountName);
                        ServiceProcessInstaller.RemoveAccountRight(intPtr, accountSid, "SeServiceLogonRight");
                    }
                    finally
                    {
                        SafeNativeMethods.LsaClose(intPtr);
                    }
                }
            }
            finally
            {
                base.Rollback(savedState);
            }
        }
    }
}
