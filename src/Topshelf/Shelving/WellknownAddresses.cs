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
    using System.Collections.Generic;
    using System.Diagnostics;
    using Exceptions;
    using Magnum.Channels;
    using Magnum.Fibers;

    public static class WellknownAddresses
    {
        public static WcfChannelHost GetHostHost(UntypedChannel hostProxy)
        {
            return new WcfChannelHost(new SynchronousFiber(), hostProxy, GetBaseAddress( GetHostPipeName()),"host");
        }

        public static WcfChannelHost GetCurrentShelfHost(ChannelAdapter myChannel)
        {
            var pipeName = GetThisShelfPipeName();

            EnsureNoDuplicatePipes(pipeName);

            var address = GetBaseAddress(pipeName);
            return new WcfChannelHost(new ThreadPoolFiber(), myChannel, address, "shelf");
        }

        static readonly List<string> _pipes = new List<string>();
        static void EnsureNoDuplicatePipes(string pipeName)
        {
            if(!_pipes.Contains(pipeName))
                _pipes.Add(pipeName);
            else
                throw new ConfigurationException("Duplicate pipe detected: '{0}'".FormatWith(pipeName));
        }

        public static UntypedChannel GetCurrentChannelProxy()
        {
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetThisShelfPipeName()), "shelf"); 
        }
        public static WcfChannelProxy GetShelfChannelProxy(AppDomain appDomain)
        {
            var friendlyName = appDomain.FriendlyName;
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetShelfPipeName(friendlyName)), "shelf");
        }
        public static UntypedChannel GetHostChannelProxy()
        {
            return new WcfChannelProxy(new ThreadPoolFiber(), GetBaseAddress(GetHostPipeName()), "host");
        }

        private static string GetShelfPipeName(string name)
        {
            return "{0}/{1}".FormatWith(GetPid(),name);
        }
        private static string GetThisShelfPipeName()
        {
            return "{0}/{1}".FormatWith(GetPid(), GetFolder());
        }
        private static string GetHostPipeName()
        {
            return "{0}/host".FormatWith(GetPid());
        }

        private static Uri GetBaseAddress(string name)
        {
            return new Uri("net.pipe://localhost/topshelf/{0}".FormatWith(name));
        }

        private static int GetPid()
        {
            return Process.GetCurrentProcess().Id;
        }
        private static string GetFolder()
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }
    }
}