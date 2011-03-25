﻿// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Stuff
{
    using System;
    using System.IO;
    using log4net.Config;
    using Topshelf;
    using Topshelf.Dashboard;


    internal class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.config"));

            var h = HostFactory.New(x =>
                {
                    x.AfterStoppingServices(n => Console.WriteLine("AfterStoppingServices action invoked, services are stopping"));

                    x.EnableDashboard();
                    
                    x.Service<TownCrier>(s =>
                        {
                            s.SetServiceName("TownCrier");
                            s.ConstructUsing(name => new TownCrier());
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                        });

                    x.RunAsLocalSystem();

                    x.SetDescription("Sample Topshelf Host");
                    x.SetDisplayName("Stuff");
                    x.SetServiceName("stuff");
                });

            h.Run();
        }
    }
}