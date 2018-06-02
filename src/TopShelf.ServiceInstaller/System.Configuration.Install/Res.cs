namespace System.Configuration.Install
{
    // System.Configuration.Install.Res
    using System.Configuration.Install;
    using System.Globalization;
    using System.Resources;
    using System.Threading;

    internal sealed class Res
    {
        internal const string InstallAbort = "InstallAbort";

        internal const string InstallException = "InstallException";

        internal const string InstallLogContent = "InstallLogContent";

        internal const string InstallFileLocation = "InstallFileLocation";

        internal const string InstallLogParameters = "InstallLogParameters";

        internal const string InstallLogNone = "InstallLogNone";

        internal const string InstallNoPublicInstallers = "InstallNoPublicInstallers";

        internal const string InstallFileNotFound = "InstallFileNotFound";

        internal const string InstallNoInstallerTypes = "InstallNoInstallerTypes";

        internal const string InstallCannotCreateInstance = "InstallCannotCreateInstance";

        internal const string InstallBadParent = "InstallBadParent";

        internal const string InstallRecursiveParent = "InstallRecursiveParent";

        internal const string InstallNullParameter = "InstallNullParameter";

        internal const string InstallDictionaryMissingValues = "InstallDictionaryMissingValues";

        internal const string InstallDictionaryCorrupted = "InstallDictionaryCorrupted";

        internal const string InstallCommitException = "InstallCommitException";

        internal const string InstallRollbackException = "InstallRollbackException";

        internal const string InstallUninstallException = "InstallUninstallException";

        internal const string InstallEventException = "InstallEventException";

        internal const string InstallInstallerNotFound = "InstallInstallerNotFound";

        internal const string InstallSeverityError = "InstallSeverityError";

        internal const string InstallSeverityWarning = "InstallSeverityWarning";

        internal const string InstallLogInner = "InstallLogInner";

        internal const string InstallLogError = "InstallLogError";

        internal const string InstallLogCommitException = "InstallLogCommitException";

        internal const string InstallLogRollbackException = "InstallLogRollbackException";

        internal const string InstallLogUninstallException = "InstallLogUninstallException";

        internal const string InstallRollback = "InstallRollback";

        internal const string InstallAssemblyHelp = "InstallAssemblyHelp";

        internal const string InstallActivityRollingBack = "InstallActivityRollingBack";

        internal const string InstallActivityUninstalling = "InstallActivityUninstalling";

        internal const string InstallActivityCommitting = "InstallActivityCommitting";

        internal const string InstallActivityInstalling = "InstallActivityInstalling";

        internal const string InstallInfoTransacted = "InstallInfoTransacted";

        internal const string InstallInfoBeginInstall = "InstallInfoBeginInstall";

        internal const string InstallInfoException = "InstallInfoException";

        internal const string InstallInfoBeginRollback = "InstallInfoBeginRollback";

        internal const string InstallInfoRollbackDone = "InstallInfoRollbackDone";

        internal const string InstallInfoBeginCommit = "InstallInfoBeginCommit";

        internal const string InstallInfoCommitDone = "InstallInfoCommitDone";

        internal const string InstallInfoTransactedDone = "InstallInfoTransactedDone";

        internal const string InstallInfoBeginUninstall = "InstallInfoBeginUninstall";

        internal const string InstallInfoUninstallDone = "InstallInfoUninstallDone";

        internal const string InstallSavedStateFileCorruptedWarning = "InstallSavedStateFileCorruptedWarning";

        internal const string IncompleteEventLog = "IncompleteEventLog";

        internal const string IncompletePerformanceCounter = "IncompletePerformanceCounter";

        internal const string PerfInvalidCategoryName = "PerfInvalidCategoryName";

        internal const string NotCustomPerformanceCategory = "NotCustomPerformanceCategory";

        internal const string RemovingInstallState = "RemovingInstallState";

        internal const string InstallUnableDeleteFile = "InstallUnableDeleteFile";

        internal const string InstallInitializeException = "InstallInitializeException";

        internal const string InstallFileDoesntExist = "InstallFileDoesntExist";

        internal const string InstallFileDoesntExistCommandLine = "InstallFileDoesntExistCommandLine";

        internal const string WinNTRequired = "WinNTRequired";

        internal const string WrappedExceptionSource = "WrappedExceptionSource";

        internal const string InvalidProperty = "InvalidProperty";

        internal const string InstallRollbackNtRun = "InstallRollbackNtRun";

        internal const string InstallCommitNtRun = "InstallCommitNtRun";

        internal const string InstallUninstallNtRun = "InstallUninstallNtRun";

        internal const string InstallInstallNtRun = "InstallInstallNtRun";

        internal const string InstallHelpMessageStart = "InstallHelpMessageStart";

        internal const string InstallHelpMessageEnd = "InstallHelpMessageEnd";

        internal const string CantAddSelf = "CantAddSelf";

        internal const string Desc_Installer_HelpText = "Desc_Installer_HelpText";

        internal const string Desc_Installer_Parent = "Desc_Installer_Parent";

        internal const string Desc_AssemblyInstaller_Assembly = "Desc_AssemblyInstaller_Assembly";

        internal const string Desc_AssemblyInstaller_CommandLine = "Desc_AssemblyInstaller_CommandLine";

        internal const string Desc_AssemblyInstaller_Path = "Desc_AssemblyInstaller_Path";

        internal const string Desc_AssemblyInstaller_UseNewContext = "Desc_AssemblyInstaller_UseNewContext";

        internal const string NotAnEventLog = "NotAnEventLog";

        internal const string CreatingEventLog = "CreatingEventLog";

        internal const string RestoringEventLog = "RestoringEventLog";

        internal const string RemovingEventLog = "RemovingEventLog";

        internal const string DeletingEventLog = "DeletingEventLog";

        internal const string LocalSourceNotRegisteredWarning = "LocalSourceNotRegisteredWarning";

        internal const string Desc_CategoryResourceFile = "Desc_CategoryResourceFile";

        internal const string Desc_CategoryCount = "Desc_CategoryCount";

        internal const string Desc_Log = "Desc_Log";

        internal const string Desc_MessageResourceFile = "Desc_MessageResourceFile";

        internal const string Desc_ParameterResourceFile = "Desc_ParameterResourceFile";

        internal const string Desc_Source = "Desc_Source";

        internal const string Desc_UninstallAction = "Desc_UninstallAction";

        internal const string NotAPerformanceCounter = "NotAPerformanceCounter";

        internal const string NewCategory = "NewCategory";

        internal const string RestoringPerformanceCounter = "RestoringPerformanceCounter";

        internal const string CreatingPerformanceCounter = "CreatingPerformanceCounter";

        internal const string RemovingPerformanceCounter = "RemovingPerformanceCounter";

        internal const string PCCategoryName = "PCCategoryName";

        internal const string PCCounterName = "PCCounterName";

        internal const string PCInstanceName = "PCInstanceName";

        internal const string PCMachineName = "PCMachineName";

        internal const string PCI_CategoryHelp = "PCI_CategoryHelp";

        internal const string PCI_Counters = "PCI_Counters";

        internal const string PCI_IsMultiInstance = "PCI_IsMultiInstance";

        internal const string PCI_UninstallAction = "PCI_UninstallAction";

        private static Res loader;

        private ResourceManager resources;

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return Res.GetLoader().resources;
            }
        }

        internal Res()
        {
            this.resources = new ResourceManager("System.Configuration.Install", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (Res.loader == null)
            {
                Res value = new Res();
                Interlocked.CompareExchange<Res>(ref Res.loader, value, (Res)null);
            }
            return Res.loader;
        }

        public static string GetString(string name, params object[] args)
        {
            Res res = Res.GetLoader();
            if (res == null)
            {
                return null;
            }
            string @string = res.resources.GetString(name, Res.Culture);
            if (args != null && args.Length != 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string text = args[i] as string;
                    if (text != null && text.Length > 1024)
                    {
                        args[i] = text.Substring(0, 1021) + "...";
                    }
                }
                return string.Format(CultureInfo.CurrentCulture, @string, args);
            }
            return @string;
        }

        public static string GetString(string name)
        {
            Res res = Res.GetLoader();
            if (res == null)
            {
                return null;
            }
            return res.resources.GetString(name, Res.Culture);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return Res.GetString(name);
        }

        public static object GetObject(string name)
        {
            Res res = Res.GetLoader();
            if (res == null)
            {
                return null;
            }
            return res.resources.GetObject(name, Res.Culture);
        }
    }
}
