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
            var t = Shelf.FindBootstrapperImplementation();
            Console.WriteLine(t);
            var o = Shelf.CreateController(t);
            Console.WriteLine(o);
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