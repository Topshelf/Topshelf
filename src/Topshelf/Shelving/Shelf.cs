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
namespace Topshelf.Shelving
{
    using System;
    using System.Linq;
    using Configuration.Dsl;
    using Magnum.Channels;
    using Model;

    public class Shelf
    {
        IServiceController _controller;
        WcfUntypedChannel _hostChannel;

        public Shelf()
        {
            Initialize();
        }

        public void Initialize()
        {
            //how do the addresses work (its a light wrapper over wcf)
            _hostChannel = new WcfUntypedChannel(new ThreadPoolFiber(), WellknownAddresses.HostAddress, "topshelf.host");

            var t = FindBootstrapperImplementation();
            var b = (Bootstrapper)Activator.CreateInstance(t);

            var cfg = new ServiceConfigurator<object>();
			
			//let the bootstrapper configure the service
			//have to do some type coearcion here
			//wonder if co/contra will help here?
            b.InitializeHostedService<object>()(cfg);
            
            //start up the service controller instance
            _controller = cfg.Create();

            //wire up all the subscriptions

            //send message to host that I am ready
        }

        static Type FindBootstrapperImplementation()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsInterface == false)
                .Where(x => typeof (Bootstrapper).IsAssignableFrom(x))
                .FirstOrDefault();

            if (type == null)
                throw new InvalidOperationException("The bootstrapper was not found.");
            return type;
        }
    }
}