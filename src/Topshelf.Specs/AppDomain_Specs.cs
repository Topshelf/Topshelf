// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Specs
{
    using System;
    using NUnit.Framework;

    //a place to learn about app-domains
    [TestFixture]
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

            var bill = (Bill)ad.CreateInstanceAndUnwrap(type.Assembly.GetName().FullName, type.FullName, true, 0, null, new []{func}, null, null, null);

            bill.ShouldNotBeNull();
            bill.Yo.ShouldEqual(3);
            bill.AppDomainName.ShouldEqual("bob");
            //bill.ThreadInfo.ShouldEqual("STA"); //fails on the nunit runner which is MTA
            Bill.Sup.ShouldEqual(2);
            //AppDomain.CurrentDomain.FriendlyName.ShouldEqual("domain-nunit.addin.dll");
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
        public string ThreadInfo
        {
            get
            {
                var a = System.Threading.Thread.CurrentThread.GetApartmentState();
                return a.ToString();
            }
        }
    }

}