using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Install
{
    public abstract class ComponentInstaller : Installer
    {
        /// <summary>When overridden in a derived class, copies all the properties that are required at install time from the specified component.</summary>
        /// <param name="component">The component to copy from. </param>
        public abstract void CopyFromComponent(IComponent component);

        /// <summary>Determines if the specified installer installs the same object as this installer.</summary>
        /// <returns>true if this installer and the installer specified by the <paramref name="otherInstaller" /> parameter install the same object; otherwise, false.</returns>
        /// <param name="otherInstaller">The installer to compare. </param>
        public virtual bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            return false;
        }
    }
}
