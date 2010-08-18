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
//namespace Topshelf.Shelving
//{
//    using System;
//    using System.Collections.Generic;
//    using log4net;
//    using Magnum.Channels;
//    using Magnum.Threading;
//    using Messages;
//
//
//	public class ShelfMaker :
//        IDisposable
//    {
//        readonly HostHost _myChannelHost;
//        readonly ChannelAdapter _myChannel;
//        readonly ReaderWriterLockedObject<Dictionary<string, ShelfService>> _shelves;
//        static readonly ILog _log = LogManager.GetLogger(typeof(ShelfMaker));
//
//        public ShelfMaker()
//        {
//            _shelves = new ReaderWriterLockedObject<Dictionary<string, ShelfService>>(new Dictionary<string, ShelfService>());
//
//            _myChannel = new ChannelAdapter();
//            _myChannelHost = WellknownAddresses.GetShelfMakerHost(_myChannel);
//
//            _myChannel.Connect(s =>
//            {
//				s.AddConsumerOf<ServiceCreated>().UsingConsumer(MarkServiceReadyAndStart);
//				s.AddConsumerOf<ServiceStopped>().UsingConsumer(MarkShelvedServiceStopped);
//				s.AddConsumerOf<ServiceFolderChanged>().UsingConsumer(ReloadShelf);
//				s.AddConsumerOf<ShelfFault>().UsingConsumer(HandleShelfFault);
//            });
//
//            _log.Debug("ShelfMaker started.");
//        }
//
//        void HandleShelfFault(ShelfFault message)
//        {
//            _log.Error("Error in shelf '{0}' -> '{1}'".FormatWith(message.ShelfName, message.Exception.Message), message.Exception);
//            ShelfService shelfStatus = GetShelfStatus(message.ShelfName);
//            if (shelfStatus == null)
//                return;
//
//            ReloadShelf(message);
//        }
//    
//    }
//}
