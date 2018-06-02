using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.ServiceProcess
{
    internal sealed class Res
    {
        internal const string RTL = "RTL";

        internal const string FileName = "FileName";

        internal const string ServiceStartedIncorrectly = "ServiceStartedIncorrectly";

        internal const string CallbackHandler = "CallbackHandler";

        internal const string OpenService = "OpenService";

        internal const string StartService = "StartService";

        internal const string StopService = "StopService";

        internal const string PauseService = "PauseService";

        internal const string ResumeService = "ResumeService";

        internal const string ControlService = "ControlService";

        internal const string ServiceName = "ServiceName";

        internal const string ServiceStartType = "ServiceStartType";

        internal const string ServiceDependency = "ServiceDependency";

        internal const string InstallService = "InstallService";

        internal const string InstallError = "InstallError";

        internal const string UserName = "UserName";

        internal const string UserPassword = "UserPassword";

        internal const string ButtonOK = "ButtonOK";

        internal const string ServiceUsage = "ServiceUsage";

        internal const string ServiceNameTooLongForNt4 = "ServiceNameTooLongForNt4";

        internal const string DisplayNameTooLong = "DisplayNameTooLong";

        internal const string NoService = "NoService";

        internal const string NoDisplayName = "NoDisplayName";

        internal const string OpenSC = "OpenSC";

        internal const string Timeout = "Timeout";

        internal const string CannotChangeProperties = "CannotChangeProperties";

        internal const string CannotChangeName = "CannotChangeName";

        internal const string NoServices = "NoServices";

        internal const string NoMachineName = "NoMachineName";

        internal const string BadMachineName = "BadMachineName";

        internal const string NoGivenName = "NoGivenName";

        internal const string CannotStart = "CannotStart";

        internal const string NotAService = "NotAService";

        internal const string NoInstaller = "NoInstaller";

        internal const string UserCanceledInstall = "UserCanceledInstall";

        internal const string UnattendedCannotPrompt = "UnattendedCannotPrompt";

        internal const string InvalidParameter = "InvalidParameter";

        internal const string FailedToUnloadAppDomain = "FailedToUnloadAppDomain";

        internal const string NotInPendingState = "NotInPendingState";

        internal const string ArgsCantBeNull = "ArgsCantBeNull";

        internal const string StartSuccessful = "StartSuccessful";

        internal const string StopSuccessful = "StopSuccessful";

        internal const string PauseSuccessful = "PauseSuccessful";

        internal const string ContinueSuccessful = "ContinueSuccessful";

        internal const string InstallSuccessful = "InstallSuccessful";

        internal const string UninstallSuccessful = "UninstallSuccessful";

        internal const string CommandSuccessful = "CommandSuccessful";

        internal const string StartFailed = "StartFailed";

        internal const string StopFailed = "StopFailed";

        internal const string PauseFailed = "PauseFailed";

        internal const string ContinueFailed = "ContinueFailed";

        internal const string SessionChangeFailed = "SessionChangeFailed";

        internal const string InstallFailed = "InstallFailed";

        internal const string UninstallFailed = "UninstallFailed";

        internal const string CommandFailed = "CommandFailed";

        internal const string ErrorNumber = "ErrorNumber";

        internal const string ShutdownOK = "ShutdownOK";

        internal const string ShutdownFailed = "ShutdownFailed";

        internal const string PowerEventOK = "PowerEventOK";

        internal const string PowerEventFailed = "PowerEventFailed";

        internal const string InstallOK = "InstallOK";

        internal const string TryToStop = "TryToStop";

        internal const string ServiceRemoving = "ServiceRemoving";

        internal const string ServiceRemoved = "ServiceRemoved";

        internal const string HelpText = "HelpText";

        internal const string CantStartFromCommandLine = "CantStartFromCommandLine";

        internal const string CantStartFromCommandLineTitle = "CantStartFromCommandLineTitle";

        internal const string CantRunOnWin9x = "CantRunOnWin9x";

        internal const string CantRunOnWin9xTitle = "CantRunOnWin9xTitle";

        internal const string CantControlOnWin9x = "CantControlOnWin9x";

        internal const string CantInstallOnWin9x = "CantInstallOnWin9x";

        internal const string InstallingService = "InstallingService";

        internal const string StartingService = "StartingService";

        internal const string SBAutoLog = "SBAutoLog";

        internal const string SBServiceName = "SBServiceName";

        internal const string SBServiceDescription = "SBServiceDescription";

        internal const string ServiceControllerDesc = "ServiceControllerDesc";

        internal const string SPCanPauseAndContinue = "SPCanPauseAndContinue";

        internal const string SPCanShutdown = "SPCanShutdown";

        internal const string SPCanStop = "SPCanStop";

        internal const string SPDisplayName = "SPDisplayName";

        internal const string SPDependentServices = "SPDependentServices";

        internal const string SPMachineName = "SPMachineName";

        internal const string SPServiceName = "SPServiceName";

        internal const string SPServicesDependedOn = "SPServicesDependedOn";

        internal const string SPStatus = "SPStatus";

        internal const string SPServiceType = "SPServiceType";

        internal const string SPStartType = "SPStartType";

        internal const string ServiceProcessInstallerAccount = "ServiceProcessInstallerAccount";

        internal const string ServiceInstallerDescription = "ServiceInstallerDescription";

        internal const string ServiceInstallerServicesDependedOn = "ServiceInstallerServicesDependedOn";

        internal const string ServiceInstallerServiceName = "ServiceInstallerServiceName";

        internal const string ServiceInstallerStartType = "ServiceInstallerStartType";

        internal const string ServiceInstallerDelayedAutoStart = "ServiceInstallerDelayedAutoStart";

        internal const string ServiceInstallerDisplayName = "ServiceInstallerDisplayName";

        internal const string Label_SetServiceLogin = "Label_SetServiceLogin";

        internal const string Label_MissmatchedPasswords = "Label_MissmatchedPasswords";

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
            this.resources = new ResourceManager("System.ServiceProcess", base.GetType().Assembly);
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
