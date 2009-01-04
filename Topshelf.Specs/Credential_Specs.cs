namespace Topshelf.Specs
{
    using System.ServiceProcess;
    using MbUnit.Framework;

    [TestFixture]
    public class Credential_Specs
    {
        [Test]
        public void Equality()
        {
            Credentials oneA = new Credentials("a", "a", ServiceAccount.LocalService);
            Credentials oneB = new Credentials("a", "a", ServiceAccount.LocalService);
            Credentials two = new Credentials("2", "a", ServiceAccount.LocalService);
            Credentials three = new Credentials("a", "2", ServiceAccount.LocalService);
            Credentials four = new Credentials("a", "a", ServiceAccount.User);

            oneA.Equals(oneA)
                .ShouldBeTrue("instance equality");

            oneA.Equals(oneB)
                .ShouldBeTrue("value equality");

            oneA.Equals(two)
                .ShouldBeFalse("username");

            oneA.Equals(three)
                .ShouldBeFalse("password");

            oneA.Equals(four)
                .ShouldBeFalse("service account");

            oneA.Equals(null)
                .ShouldBeFalse("null");
        }
    }
}