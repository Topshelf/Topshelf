namespace Topshelf.Shelving
{
    using System;
    using System.Runtime.Remoting;

    public class ShelfMaker
    {
        public void MakeShelf(string name)
        {
            AppDomainSetup settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";
            AppDomain ad = AppDomain.CreateDomain(name, null, settings);
            Type type = typeof (Shelf);
            ObjectHandle s = ad.CreateInstance(type.Assembly.GetName().FullName, type.FullName, true, 0, null, null,
                                               null, null);
        }
    }
}