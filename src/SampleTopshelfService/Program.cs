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
namespace SampleTopshelfService
{
    using System;
    using Topshelf;

    class Program
    {
        static int Main()
        {
            return (int)HostFactory.Run(x =>
                {
                    x.UseLog4Net("log4net.config");

                    x.UseAssemblyInfoForServiceInfo();

                    bool throwOnStart = false;
                    bool throwOnStop = false;
                    bool throwUnhandled = false;

                    x.Service(settings => new SampleService(throwOnStart, throwOnStop, throwUnhandled), s =>
                    {
                        s.BeforeStartingService(_ => Console.WriteLine("BeforeStart"));
                        s.BeforeStoppingService(_ => Console.WriteLine("BeforeStop"));
                    });

                    x.SetStartTimeout(TimeSpan.FromSeconds(10));
                    x.SetStopTimeout(TimeSpan.FromSeconds(10));

                    x.EnableServiceRecovery(r =>
                        {
                            r.RestartService(3);
                            r.RunProgram(7, "ping google.com");
                            r.RestartComputer(5, "message");

                            r.OnCrashOnly();
                            r.SetResetPeriod(2);
                        });

                    x.AddCommandLineSwitch("throwonstart", v => throwOnStart = v);
                    x.AddCommandLineSwitch("throwonstop", v => throwOnStop = v);
                    x.AddCommandLineSwitch("throwunhandled", v => throwUnhandled = v);
                });
        }

        void SansInterface()
        {
            HostFactory.New(x =>
                {
                    // can define services without the interface dependency as well, this is just for
                    // show and not used in this sample.
                    x.Service<SampleSansInterfaceService>(s =>
                        {
                            s.ConstructUsing(() => new SampleSansInterfaceService());
                            s.WhenStarted(v => v.Start());
                            s.WhenStarted(v => v.Stop());
                        });
                });
        }
    }
}