namespace Topshelf.Shelving
{
    using System;
    using System.Configuration;

    public class ShelfConfiguration :
        ConfigurationSection
    {
        public static ShelfConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("ShelfConfiguration") as ShelfConfiguration;
        }

        public static ShelfConfiguration GetConfig(string fileName)
        {
            return ConfigurationManager.OpenExeConfiguration(fileName).GetSection("ShelfConfiguration") as ShelfConfiguration;
        }

        [ConfigurationProperty("Bootstrapper", IsRequired = true)]
        [SubclassTypeValidator(typeof(Bootstrapper<>))]
        public Type BootstrapperType
        {
            get
            {
                return Type.GetType((string)this["Bootstrapper"]);
            }
        }
    }
}