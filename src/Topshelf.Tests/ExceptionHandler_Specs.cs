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
    using System;
    using NUnit.Framework;


    [TestFixture]
    public class Exception_callback
    {
        private static readonly string StartExceptionMessage = "Throw on Start Requested";
        private static readonly string StopExceptionMessage = "Throw on Stop Requested";

        [Test]
        public void Should_be_called_when_exception_thrown_in_Start_method()
        {
            var sawExceptionInStart = false;
            var sawExceptionInStop = false;

            var exitCode = HostFactory.Run(x =>
                {
                    x.UseTestHost();

                    x.Service(settings => new ExceptionThrowingService(true, false));

                    x.OnException(ex =>
                    {
                        if (ex.Message == StartExceptionMessage)
                        {
                            sawExceptionInStart = true;
                        }

                        if (ex.Message == StopExceptionMessage)
                        {
                            sawExceptionInStop = true;
                        }
                    });
                });

            Assert.IsTrue(sawExceptionInStart);
            Assert.IsFalse(sawExceptionInStop);
            Assert.AreEqual(TopshelfExitCode.StartServiceFailed, exitCode);
        }

        [Test]
        public void Should_be_called_when_exception_thrown_in_Stop_method()
        {
            var sawExceptionInStart = false;
            var sawExceptionInStop = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(false, true));

                x.OnException(ex =>
                {
                    if (ex.Message == StartExceptionMessage)
                    {
                        sawExceptionInStart = true;
                    }

                    if (ex.Message == StopExceptionMessage)
                    {
                        sawExceptionInStop = true;
                    }
                });
            });

            Assert.IsFalse(sawExceptionInStart);
            Assert.IsTrue(sawExceptionInStop);
            Assert.AreEqual(TopshelfExitCode.StopServiceFailed, exitCode);
        }

        [Test]
        public void Should_not_be_called_when_no_exceptions_thrown()
        {
            var sawException = false;

            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(false, false));

                x.OnException(ex =>
                {
                    sawException = true;
                });
            });

            Assert.IsFalse(sawException);
            Assert.AreEqual(TopshelfExitCode.Ok, exitCode);
        }

        [Test]
        public void Should_not_prevent_default_action_when_not_set()
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(true, false));
            });

            Assert.AreEqual(TopshelfExitCode.StartServiceFailed, exitCode);

            exitCode = HostFactory.Run(x =>
            {
                x.UseTestHost();

                x.Service(settings => new ExceptionThrowingService(false, true));
            });

            Assert.AreEqual(TopshelfExitCode.StopServiceFailed, exitCode);

        }

        /// <summary>
        /// A simple service that can be configured to throw exceptions while starting or stopping.
        /// </summary>
        class ExceptionThrowingService : ServiceControl
        {
            readonly bool _throwOnStart;
            readonly bool _throwOnStop;

            public ExceptionThrowingService(bool throwOnStart, bool throwOnStop)
            {
                _throwOnStart = throwOnStart;
                _throwOnStop = throwOnStop;
            }

            public bool Start(HostControl hostControl)
            {
                if (_throwOnStart)
                {
                    throw new InvalidOperationException(StartExceptionMessage);
                }

                return true;
            }

            public bool Stop(HostControl hostControl)
            {
                if (_throwOnStop)
                {
                    throw new InvalidOperationException(StopExceptionMessage);
                }

                return true;
            }
        }
    }
}