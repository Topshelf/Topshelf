using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Install
{
    public class TransactedInstaller : Installer
    {
        public override void Install(IDictionary savedState)
        {
            if (base.Context == null)
            {
                base.Context = new InstallContext();
            }
            base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransacted"));
            try
            {
                bool flag = true;
                try
                {
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginInstall"));
                    base.Install(savedState);
                }
                catch (Exception ex)
                {
                    flag = false;
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoException"));
                    Installer.LogException(ex, base.Context);
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginRollback"));
                    try
                    {
                        this.Rollback(savedState);
                    }
                    catch (Exception)
                    {
                    }
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoRollbackDone"));
                    throw new InvalidOperationException(Res.GetString("InstallRollback"), ex);
                }
                if (flag)
                {
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginCommit"));
                    try
                    {
                        this.Commit(savedState);
                    }
                    finally
                    {
                        base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoCommitDone"));
                    }
                }
            }
            finally
            {
                base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransactedDone"));
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            if (base.Context == null)
            {
                base.Context = new InstallContext();
            }
            base.Context.LogMessage(Environment.NewLine + Environment.NewLine + Res.GetString("InstallInfoBeginUninstall"));
            try
            {
                base.Uninstall(savedState);
            }
            finally
            {
                base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoUninstallDone"));
            }
        }
    }
}