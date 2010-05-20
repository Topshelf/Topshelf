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
    using System.Runtime.Remoting;
    using Magnum.Channels;

    public class ShelfStatus
    {
        private WcfUntypedChannel _shelfChannel = null;

        public Func<AppDomain, WcfUntypedChannel> ShelfChannelBuilder { private get; set; }
        public string ShelfName { get; set; }
        public ObjectHandle ObjectHandle { get; set; }
        public Shelf RemoteShelf { get; set; }
        public WcfUntypedChannel ShelfChannel
        {
            get { return _shelfChannel ?? (_shelfChannel = ShelfChannelBuilder(AppDomain)); }
        }
        public AppDomain AppDomain { get; set; }
        public ShelfState CurrentState { get; set; }
    }
}