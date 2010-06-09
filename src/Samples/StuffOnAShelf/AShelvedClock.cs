// Copyright 2007-2010 The Apache Software Foundation.
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
namespace StuffOnAShelf
{
    using System;
    using System.IO;
    using System.Timers;
    using log4net;
    using log4net.Config;
    using Topshelf.Configuration.Dsl;
    using Topshelf.Shelving;

    public class AShelvedClock :
        Bootstrapper<TheClock>
    {
        public void InitializeHostedService(IServiceConfigurator<TheClock> cfg)
        {
            cfg.HowToBuildService(n => new TheClock());
            cfg.WhenStarted(s =>
            {
                XmlConfigurator.Configure(new FileInfo(".\\clock.log4net.config"));
                s.Start();
            });
            cfg.WhenStopped(s => s.Stop());
        }
    }

    public class TheClock
    {
        readonly Timer _timer;
        readonly ILog _log = LogManager.GetLogger(typeof (TheClock));

        public TheClock()
        {
            _timer = new Timer(1000) {AutoReset = true};
            _timer.Elapsed += (sender, eventArgs) => _log.Info(DateTime.Now);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}