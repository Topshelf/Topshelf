// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.Tests
{
    using NUnit.Framework;


    [TestFixture]
    public class When_the_service_start_is_canceled
    {
        [Test]
        public void Should_not_start_the_service()
        {
            bool started = false;

            var exitCode = HostFactory.Run(x =>
                {
                    x.UseTestHost();

                    x.Service(settings => new MyService(), s =>
                        {
                            s.BeforeStartingService(hsc => hsc.CancelStart());
                            s.AfterStartingService(hsc => { started = true; });
                        });
                });

            Assert.IsFalse(started);
            Assert.AreEqual(TopshelfExitCode.ServiceControlRequestFailed, exitCode);
        }


        class MyService : ServiceControl
        {
            public bool Start(HostControl hostControl)
            {
                return true;
            }

            public bool Stop(HostControl hostControl)
            {
                return true;
            }
        }
    }
}