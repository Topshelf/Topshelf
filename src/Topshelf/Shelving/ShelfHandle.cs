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
    using System.Diagnostics;
    using System.Runtime.Remoting;
    using System.Threading;
    using Magnum.Channels;

    [DebuggerDisplay("{ShelfName}: {CurrentState}")]
    public class ShelfHandle :
        IDisposable
    {
        private WcfChannelProxy _shelfChannel = null;
		public WcfChannelProxy ShelfChannel
        {
            get { return _shelfChannel ?? (_shelfChannel = ShelfChannelBuilder(AppDomain)); }
        }

        WcfChannelProxy _serviceChannel = null;
        public WcfChannelProxy ServiceChannel
        {
            get { return _serviceChannel ?? (_serviceChannel = ServiceChannelBuilder(AppDomain)); }
        }
        public Func<AppDomain, WcfChannelProxy> ShelfChannelBuilder { private get; set; }
        public Func<AppDomain, WcfChannelProxy> ServiceChannelBuilder { private get; set; }

        // Do we need to hold onto this handle at all?
        internal ObjectHandle ObjectHandle { get; set; }
        
        public string ShelfName { get; set; }
		public AppDomain AppDomain { get; set; }
        public ShelfState CurrentState { get; set; }

        /// <summary>
        /// Used for shutting down
        /// </summary>
        // Is there a better place to keep this? Something inside the ShelfMaker?
        public ManualResetEvent StopHandle { get; set; }
        
        public void Dispose()
        {
            if (StopHandle != null)
                StopHandle.Close();
        }
    }
}