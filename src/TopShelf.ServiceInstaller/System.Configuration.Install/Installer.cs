using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Install
{
    public class Installer : System.ComponentModel.Component
    {
        internal Installer parent;
        private InstallerCollection installers;

        public InstallContext Context { get; set; }
        public InstallerCollection Installers
        {
            get
            {
                if (this.installers == null)
                {
                    this.installers = new InstallerCollection(this);
                }

                return this.installers;
            }
        }

        [ResDescription("Desc_Installer_HelpText")]
        public virtual string HelpText
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < this.Installers.Count; i++)
                {
                    string helpText = this.Installers[i].HelpText;
                    if (helpText.Length > 0)
                    {
                        stringBuilder.Append("\r\n");
                        stringBuilder.Append(helpText);
                    }
                }
                return stringBuilder.ToString();
            }
        }

        [TypeConverter(typeof(InstallerParentConverter))]
        [ResDescription("Desc_Installer_Parent")]
        public Installer Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                if (value == this)
                {
                    throw new InvalidOperationException(Res.GetString("InstallBadParent"));
                }
                if (value != this.parent)
                {
                    if (value != null && this.InstallerTreeContains(value))
                    {
                        throw new InvalidOperationException(Res.GetString("InstallRecursiveParent"));
                    }
                    if (this.parent != null)
                    {
                        int num = this.parent.Installers.IndexOf(this);
                        if (num != -1)
                        {
                            this.parent.Installers.RemoveAt(num);
                        }
                    }
                    this.parent = value;
                    if (this.parent != null && !this.parent.Installers.Contains(this))
                    {
                        this.parent.Installers.Add(this);
                    }
                }
            }
        }
        public virtual void Commit(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", "savedState"));
            }
            if (savedState["_reserved_lastInstallerAttempted"] != null && savedState["_reserved_nestedSavedStates"] != null)
            {
                Exception ex = null;
                try
                {
                    this.OnCommitting(savedState);
                }
                catch (Exception ex2)
                {
                    this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnCommitting", ex2);
                    this.Context.LogMessage(Res.GetString("InstallCommitException"));
                    ex = ex2;
                }
                int num = (int)savedState["_reserved_lastInstallerAttempted"];
                IDictionary[] array = (IDictionary[])savedState["_reserved_nestedSavedStates"];
                if (num + 1 == array.Length && num < this.Installers.Count)
                {
                    for (int i = 0; i < this.Installers.Count; i++)
                    {
                        this.Installers[i].Context = this.Context;
                    }
                    for (int j = 0; j <= num; j++)
                    {
                        try
                        {
                            this.Installers[j].Commit(array[j]);
                        }
                        catch (Exception ex3)
                        {
                            if (!this.IsWrappedException(ex3))
                            {
                                this.Context.LogMessage(Res.GetString("InstallLogCommitException", this.Installers[j].ToString()));
                                Installer.LogException(ex3, this.Context);
                                this.Context.LogMessage(Res.GetString("InstallCommitException"));
                            }
                            ex = ex3;
                        }
                    }
                    savedState["_reserved_nestedSavedStates"] = array;
                    savedState.Remove("_reserved_lastInstallerAttempted");
                    try
                    {
                        this.OnCommitted(savedState);
                    }
                    catch (Exception ex4)
                    {
                        this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnCommitted", ex4);
                        this.Context.LogMessage(Res.GetString("InstallCommitException"));
                        ex = ex4;
                    }
                    if (ex == null)
                    {
                        return;
                    }
                    Exception ex5 = ex;
                    if (!this.IsWrappedException(ex))
                    {
                        ex5 = new InstallException(Res.GetString("InstallCommitException"), ex);
                        ex5.Source = "WrappedExceptionSource";
                    }
                    throw ex5;
                }
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", "savedState"));
            }
            throw new ArgumentException(Res.GetString("InstallDictionaryMissingValues", "savedState"));
        }

        public virtual void Install(IDictionary stateSaver)
        {
            if (stateSaver == null)
            {
                throw new ArgumentNullException(nameof(stateSaver));
            }
            try
            {
                this.OnBeforeInstall(stateSaver);
            }
            catch (Exception ex)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityError"), "OnBeforeInstall", ex);
                throw new InvalidOperationException(Res.GetString("InstallEventException", "OnBeforeInstall", base.GetType().FullName), ex);
            }
            int num = -1;
            List<IDictionary> arrayList = new List<IDictionary>();
            try
            {
                for (int i = 0; i < this.Installers.Count; i++)
                {
                    this.Installers[i].Context = this.Context;
                }
                for (int j = 0; j < this.Installers.Count; j++)
                {
                    Installer installer = this.Installers[j];
                    IDictionary dictionary = new Hashtable();
                    try
                    {
                        num = j;
                        installer.Install(dictionary);
                    }
                    finally
                    {
                        arrayList.Add(dictionary);
                    }
                }
            }
            finally
            {
                stateSaver.Add("_reserved_lastInstallerAttempted", num);
                stateSaver.Add("_reserved_nestedSavedStates", arrayList.ToArray());
            }
            try
            {
                this.OnAfterInstall(stateSaver);
            }
            catch (Exception ex2)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityError"), "OnAfterInstall", ex2);
                throw new InvalidOperationException(Res.GetString("InstallEventException", "OnAfterInstall", base.GetType().FullName), ex2);
            }
        }

        public virtual void Rollback(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", "savedState"));
            }
            if (savedState["_reserved_lastInstallerAttempted"] != null && savedState["_reserved_nestedSavedStates"] != null)
            {
                Exception ex = null;
                try
                {
                    this.OnBeforeRollback(savedState);
                }
                catch (Exception ex2)
                {
                    this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnBeforeRollback", ex2);
                    this.Context.LogMessage(Res.GetString("InstallRollbackException"));
                    ex = ex2;
                }
                int num = (int)savedState["_reserved_lastInstallerAttempted"];
                IDictionary[] array = (IDictionary[])savedState["_reserved_nestedSavedStates"];
                if (num + 1 == array.Length && num < this.Installers.Count)
                {
                    for (int num2 = this.Installers.Count - 1; num2 >= 0; num2--)
                    {
                        this.Installers[num2].Context = this.Context;
                    }
                    for (int num3 = num; num3 >= 0; num3--)
                    {
                        try
                        {
                            this.Installers[num3].Rollback(array[num3]);
                        }
                        catch (Exception ex3)
                        {
                            if (!this.IsWrappedException(ex3))
                            {
                                this.Context.LogMessage(Res.GetString("InstallLogRollbackException", this.Installers[num3].ToString()));
                                Installer.LogException(ex3, this.Context);
                                this.Context.LogMessage(Res.GetString("InstallRollbackException"));
                            }
                            ex = ex3;
                        }
                    }
                    try
                    {
                        this.OnAfterRollback(savedState);
                    }
                    catch (Exception ex4)
                    {
                        this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnAfterRollback", ex4);
                        this.Context.LogMessage(Res.GetString("InstallRollbackException"));
                        ex = ex4;
                    }
                    if (ex == null)
                    {
                        return;
                    }
                    Exception ex5 = ex;
                    if (!this.IsWrappedException(ex))
                    {
                        ex5 = new InstallException(Res.GetString("InstallRollbackException"), ex);
                        ex5.Source = "WrappedExceptionSource";
                    }
                    throw ex5;
                }
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", "savedState"));
            }
            throw new ArgumentException(Res.GetString("InstallDictionaryMissingValues", "savedState"));
        }

        public virtual void Uninstall(IDictionary savedState)
        {
            Exception ex = null;
            try
            {
                this.OnBeforeUninstall(savedState);
            }
            catch (Exception ex2)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnBeforeUninstall", ex2);
                this.Context.LogMessage(Res.GetString("InstallUninstallException"));
                ex = ex2;
            }
            IDictionary[] array;
            if (savedState != null)
            {
                array = (IDictionary[])savedState["_reserved_nestedSavedStates"];
                if (array != null && array.Length == this.Installers.Count)
                {
                    goto IL_0091;
                }
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", "savedState"));
            }
            array = new IDictionary[this.Installers.Count];
            goto IL_0091;
            IL_0091:
            for (int num = this.Installers.Count - 1; num >= 0; num--)
            {
                this.Installers[num].Context = this.Context;
            }
            for (int num2 = this.Installers.Count - 1; num2 >= 0; num2--)
            {
                try
                {
                    this.Installers[num2].Uninstall(array[num2]);
                }
                catch (Exception ex3)
                {
                    if (!this.IsWrappedException(ex3))
                    {
                        this.Context.LogMessage(Res.GetString("InstallLogUninstallException", this.Installers[num2].ToString()));
                        Installer.LogException(ex3, this.Context);
                        this.Context.LogMessage(Res.GetString("InstallUninstallException"));
                    }
                    ex = ex3;
                }
            }
            try
            {
                this.OnAfterUninstall(savedState);
            }
            catch (Exception ex4)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnAfterUninstall", ex4);
                this.Context.LogMessage(Res.GetString("InstallUninstallException"));
                ex = ex4;
            }
            if (ex == null)
            {
                return;
            }
            Exception ex5 = ex;
            if (!this.IsWrappedException(ex))
            {
                ex5 = new InstallException(Res.GetString("InstallUninstallException"), ex);
                ex5.Source = "WrappedExceptionSource";
            }
            throw ex5;
        }

#region Event Handlers

        private InstallEventHandler afterCommitHandler;
        private InstallEventHandler afterInstallHandler;
        private InstallEventHandler afterRollbackHandler;
        private InstallEventHandler afterUninstallHandler;
        private InstallEventHandler beforeCommitHandler;
        private InstallEventHandler beforeInstallHandler;
        private InstallEventHandler beforeRollbackHandler;
        private InstallEventHandler beforeUninstallHandler;

        public event InstallEventHandler Committed
        {
            add
            {
                this.afterCommitHandler = (InstallEventHandler)Delegate.Combine(this.afterCommitHandler, value);
            }
            remove
            {
                this.afterCommitHandler = (InstallEventHandler)Delegate.Remove(this.afterCommitHandler, value);
            }
        }

        /// <summary>Occurs after the <see cref="M:System.Configuration.Install.Installer.Install(System.Collections.IDictionary)" /> methods of all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have run.</summary>
        public event InstallEventHandler AfterInstall
        {
            add
            {
                this.afterInstallHandler = (InstallEventHandler)Delegate.Combine(this.afterInstallHandler, value);
            }
            remove
            {
                this.afterInstallHandler = (InstallEventHandler)Delegate.Remove(this.afterInstallHandler, value);
            }
        }

        /// <summary>Occurs after the installations of all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back.</summary>
        public event InstallEventHandler AfterRollback
        {
            add
            {
                this.afterRollbackHandler = (InstallEventHandler)Delegate.Combine(this.afterRollbackHandler, value);
            }
            remove
            {
                this.afterRollbackHandler = (InstallEventHandler)Delegate.Remove(this.afterRollbackHandler, value);
            }
        }

        /// <summary>Occurs after all the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property perform their uninstallation operations.</summary>
        public event InstallEventHandler AfterUninstall
        {
            add
            {
                this.afterUninstallHandler = (InstallEventHandler)Delegate.Combine(this.afterUninstallHandler, value);
            }
            remove
            {
                this.afterUninstallHandler = (InstallEventHandler)Delegate.Remove(this.afterUninstallHandler, value);
            }
        }

        /// <summary>Occurs before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property committ their installations.</summary>
        public event InstallEventHandler Committing
        {
            add
            {
                this.beforeCommitHandler = (InstallEventHandler)Delegate.Combine(this.beforeCommitHandler, value);
            }
            remove
            {
                this.beforeCommitHandler = (InstallEventHandler)Delegate.Remove(this.beforeCommitHandler, value);
            }
        }

        /// <summary>Occurs before the <see cref="M:System.Configuration.Install.Installer.Install(System.Collections.IDictionary)" /> method of each installer in the installer collection has run.</summary>
        public event InstallEventHandler BeforeInstall
        {
            add
            {
                this.beforeInstallHandler = (InstallEventHandler)Delegate.Combine(this.beforeInstallHandler, value);
            }
            remove
            {
                this.beforeInstallHandler = (InstallEventHandler)Delegate.Remove(this.beforeInstallHandler, value);
            }
        }

        /// <summary>Occurs before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back.</summary>
        public event InstallEventHandler BeforeRollback
        {
            add
            {
                this.beforeRollbackHandler = (InstallEventHandler)Delegate.Combine(this.beforeRollbackHandler, value);
            }
            remove
            {
                this.beforeRollbackHandler = (InstallEventHandler)Delegate.Remove(this.beforeRollbackHandler, value);
            }
        }

        /// <summary>Occurs before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property perform their uninstall operations.</summary>
        public event InstallEventHandler BeforeUninstall
        {
            add
            {
                this.beforeUninstallHandler = (InstallEventHandler)Delegate.Combine(this.beforeUninstallHandler, value);
            }
            remove
            {
                this.beforeUninstallHandler = (InstallEventHandler)Delegate.Remove(this.beforeUninstallHandler, value);
            }
        }
        protected virtual void OnCommitted(IDictionary savedState)
        {
            this.afterCommitHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.AfterInstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property have completed their installations. </param>
        protected virtual void OnAfterInstall(IDictionary savedState)
        {
            this.afterInstallHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.AfterRollback" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back. </param>
        protected virtual void OnAfterRollback(IDictionary savedState)
        {
            this.afterRollbackHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.AfterUninstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer after all the installers contained in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are uninstalled. </param>
        protected virtual void OnAfterUninstall(IDictionary savedState)
        {
            this.afterUninstallHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.Committing" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are committed. </param>
        protected virtual void OnCommitting(IDictionary savedState)
        {
            this.beforeCommitHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.BeforeInstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are installed. This <see cref="T:System.Collections.IDictionary" /> object should be empty at this point. </param>
        protected virtual void OnBeforeInstall(IDictionary savedState)
        {
            this.beforeInstallHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.BeforeRollback" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property are rolled back. </param>
        protected virtual void OnBeforeRollback(IDictionary savedState)
        {
            this.beforeRollbackHandler?.Invoke(this, new InstallEventArgs(savedState));
        }

        /// <summary>Raises the <see cref="E:System.Configuration.Install.Installer.BeforeUninstall" /> event.</summary>
        /// <param name="savedState">An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property uninstall their installations. </param>
        protected virtual void OnBeforeUninstall(IDictionary savedState)
        {
            this.beforeUninstallHandler?.Invoke(this, new InstallEventArgs(savedState));
        }
#endregion

        private void WriteEventHandlerError(string severity, string eventName, Exception e)
        {
            this.Context.LogMessage(Res.GetString("InstallLogError", severity, eventName, base.GetType().FullName));
            Installer.LogException(e, this.Context);
        }

        internal static void LogException(Exception e, InstallContext context)
        {
            bool flag = true;
            while (e != null)
            {
                if (flag)
                {
                    context.LogMessage(e.GetType().FullName + ": " + e.Message);
                    flag = false;
                }
                else
                {
                    context.LogMessage(Res.GetString("InstallLogInner", e.GetType().FullName, e.Message));
                }
                if (context.IsParameterTrue("showcallstack"))
                {
                    context.LogMessage(e.StackTrace);
                }
                e = e.InnerException;
            }
        }

        private bool IsWrappedException(Exception e)
        {
            if (e is InstallException && e.Source == "WrappedExceptionSource")
            {
                return e.TargetSite.ReflectedType == typeof(Installer);
            }
            return false;
        }

        internal bool InstallerTreeContains(Installer target)
        {
            if (this.Installers.Contains(target))
            {
                return true;
            }
            foreach (Installer installer in this.Installers)
            {
                if (installer.InstallerTreeContains(target))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
