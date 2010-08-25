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
    using Magnum.Channels.Configuration;


    public class HostHost :
        IDisposable
    {
        readonly UntypedChannel _input;
        readonly ChannelConnection _inputConnection;

        public HostHost(UntypedChannel inputChannel, Uri address, string endpoint)
        {
            _input = inputChannel;
            _inputConnection = _input.Connect(x =>
                {
                    x.ReceiveFromWcfChannel(address, endpoint)
                        .HandleOnCallingThread();
                });
        }

        public ChannelConnection Connect(Action<ConnectionConfigurator> cfg)
        {
            return _input.Connect(cfg);
        }

        public void Dispose()
        {
            if (_inputConnection != null)
                _inputConnection.Dispose();
        }
    }
}