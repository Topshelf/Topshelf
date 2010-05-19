namespace Topshelf.Specs
{
    using System;
    using System.Linq;
    using Model;
    using Shelving;
    using Topshelf.Configuration.Dsl;

    public class TypeCoercionPlay
    {
        public void TestIt()
        {
            var t = Shelf.FindBootstrapperImplementation(null);
            Console.WriteLine(t);
        }

    }

    public class BS :Bootstrapper<string>
    {
        public void InitializeHostedService(IServiceConfigurator<string> cfg)
        {
            throw new NotImplementedException();
        }
    }
}