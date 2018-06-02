using System.Collections;

namespace System.Configuration.Install
{

    public delegate void InstallEventHandler(object sender, InstallEventArgs e);

    public class InstallEventArgs
    {
        public IDictionary SavedSate { get; }

        public InstallEventArgs()
        {
        }

        public InstallEventArgs(IDictionary savedSate)
        {
            this.SavedSate = savedSate;
        }
    }
}
