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
    using Magnum.Channels;


    public class HostProxy :
        IDisposable
    {
        ChannelConnection _connection;
        readonly ChannelAdapter _proxyChannel;

        public HostProxy(Uri address, string endpoint)
        {
            _proxyChannel = new ChannelAdapter();
            _connection = _proxyChannel.Connect(cc =>
                {
                    cc.SendToWcfChannel(address, endpoint).ExecuteOnThreadPoolFiber();
                });
        }

        public void Send<T>(T message)
        {
            _proxyChannel.Send(message);
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}