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
namespace Topshelf.Shelving
{
    using System;
    using System.Diagnostics;
    using Magnum.Channels;
    using Magnum.Fibers;


    public static class WellknownAddresses
    {
        const string ServiceCoordinatorEndpoint = "ServiceCoordinator";
        const string ShelfMakerEndpoint = "ShelfMaker";

        public static WcfChannelHost GetServiceCoordinatorHost(UntypedChannel hostProxy)
        {
            return new WcfChannelHost(new SynchronousFiber(), hostProxy, GetBaseAddress(GetServiceControllerPipeName()), ServiceCoordinatorEndpoint);
        }

        public static WcfChannelHost GetShelfMakerHost(UntypedChannel hostProxy)
        {
            return new WcfChannelHost(new SynchronousFiber(), hostProxy, GetBaseAddress(GetShelfMakerPipeName()), ShelfMakerEndpoint);
        }

        public static WcfChannelHost GetCurrentShelfHost(ChannelAdapter myChannel)
        {
            var pipeName = GetThisShelfPipeName();

            var address = GetBaseAddress(pipeName);
            return new WcfChannelHost(new ThreadPoolFiber(), myChannel, address, "shelf");
        }

        public static WcfChannelHost GetCurrentServiceHost(ChannelAdapter myChannel, string serviceName)
        {
            var pipeName = GetThisShelfPipeName();

            var address = GetBaseAddress(pipeName);
            return new WcfChannelHost(new ThreadPoolFiber(), myChannel, address, serviceName);
        }

        public static WcfChannelProxy GetShelfChannelProxy(AppDomain appDomain)
        {
            var friendlyName = appDomain.FriendlyName;
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetShelfPipeName(friendlyName)), "shelf");
        }

        public static WcfChannelProxy GetServiceChannelProxy(AppDomain appDomain, string serviceName)
        {
            var friendlyName = appDomain.FriendlyName;
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetShelfPipeName(friendlyName)),
                                       serviceName);
        }

        public static UntypedChannel GetServiceCoordinatorProxy()
        {
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetServiceControllerPipeName()),
                                       ServiceCoordinatorEndpoint);
        }
        
        static string GetServiceControllerPipeName()
        {
            return "{0}/servicecontroller".FormatWith(GetPid());
        }

        public static UntypedChannel GetShelfMakerProxy()
        {
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetShelfMakerPipeName()),
                                       ShelfMakerEndpoint);
        }

        static string GetShelfMakerPipeName()
        {
            return "{0}/shelfmaker".FormatWith(GetPid());
        }

        static string GetShelfPipeName(string name)
        {
            return "{0}/{1}".FormatWith(GetPid(), name);
        }

        static string GetThisShelfPipeName()
        {
            return "{0}/{1}".FormatWith(GetPid(), GetFolder());
        }

        static Uri GetBaseAddress(string name)
        {
            return new Uri("net.pipe://localhost/topshelf/{0}".FormatWith(name));
        }

        static int GetPid()
        {
            return Process.GetCurrentProcess().Id;
        }

        static string GetFolder()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }
    }
}