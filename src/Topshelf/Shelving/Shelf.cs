namespace Topshelf.Shelving
{
    using System;
    using System.IO;
    using System.Linq;

    public class Shelf
    {
        public Shelf()
        {
            Initialize();
        }

        public void Initialize()
        {
            //how to bootstap? do I assume an interface?
            File.WriteAllText(".\\bob.txt", WellknownAddresses.CurrentShelfAddress.ToString());
        }

        static Type FindBootstrapperImplementation()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsInterface == false)
                .Where(x => typeof(Bootstrapper).IsAssignableFrom(x))
                .FirstOrDefault();

            if (type == null)
                throw new InvalidOperationException("The bootstrapper was not found.");
            return type;
        }
    }
}