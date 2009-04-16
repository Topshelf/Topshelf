namespace Topshelf.Specs
{
    using Internal;
    using MbUnit.Framework;

    [TestFixture]
    public class ArgParsing_Specs
    {
        string[] _args;

        [SetUp]
        public void Establish_Context()
        {
            _args = new[] {"/install", "/instance:bob"};
        }

        [Test]
        public void InstanceName()
        {
            Parser.Args a = Parser.ParseArgs(_args);
            a.Install.ShouldBeTrue();
            a.InstanceName.ShouldEqual("bob");
        }
    }
}