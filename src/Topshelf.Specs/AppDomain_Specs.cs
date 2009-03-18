namespace Topshelf.Specs
{
    using System;
    using MbUnit.Framework;

    public class AppDomain_Specs
    {
        [Test]
        public void NAME()
        {
            var settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";
            var ad = AppDomain.CreateDomain("bob", null, settings);
            var type = typeof (Bill);
            Func<int> func = () => 3;

            var bill = ad.CreateInstanceAndUnwrap<Bill>(func);
            bill.ShouldNotBeNull();
            bill.Yo.ShouldEqual(3);
            bill.AppDomainName.ShouldEqual("bob");
            Bill.Sup.ShouldEqual(2);
            AppDomain.CurrentDomain.FriendlyName.ShouldEqual("IsolatedAppDomainHost");
        }
    }

    public class Bill : MarshalByRefObject {
        public Bill(Func<int> i)
        {
            Yo = i();
            Sup = i();
        }
        public static int Sup = 2;
        public int Yo;
        public string AppDomainName
        {
            get { return AppDomain.CurrentDomain.FriendlyName; }
        }
    }

}