// Copyright 2007-2012 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// his file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Model
{
    using System;
    using System.Diagnostics;
    using Logging;
    using Magnum.Extensions;
    using Messages;
    using Stact.Workflow;


    [DebuggerDisplay("PROCESS:{Name}")]
    public class ProcessServiceController : IServiceController
    {
        static readonly ILog _log = Logger.Get("Topshelf.Model.ProcessServiceController");

        readonly string _name;
        ProcessReference _reference;
        readonly PublishChannel _publish;

        public Type ServiceType
        {
            get { return typeof(Shelf); }
        }

        public State CurrentState { get; set; }

        public string Name
        {
            get { return _name; }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Create()
        {
            try
            {

                _log.DebugFormat("[Process:{0}] Create", _name);

                //possible file system check first
                _reference = new ProcessReference();
                _reference.Create();

            }
            catch (Exception ex)
            {
                _log.Error("cannot create process shelf", ex);

                _publish.Send(new ServiceFault(_name, ex));
            }
        }

        public void Start()
        {
            _log.DebugFormat("[Process:{0}] Start", _name);

            Send(new StartService(_name));
        }

        public void Stop()
        {
            _log.DebugFormat("[Shelf:{0}] Stop", _name);

            Send(new StopService(_name));
        }

        public void Pause()
        {
            _log.DebugFormat("[Process:{0}] Pause", _name);

            Send(new PauseService(_name));
        }

        public void Continue()
        {
            _log.DebugFormat("[Process:{0}] Continue", _name);

            Send(new ContinueService(_name));
        }

        public void Unload()
        {
            _log.DebugFormat("[Process:{0}] {1}", _name, "Unloading");
            _publish.Send(new ServiceUnloading(_name));

            if (_reference != null)
            {
                Send(new UnloadService(_name));
                _reference.Unload();
            }
            else
            {
                _publish.Send(new ServiceUnloaded(_name));
                _log.WarnFormat("[Process:{0}] {1}", _name, "Was already unloaded");
            }
        }

        void Send<T>(T message)
        {
            if (_reference == null)
            {
                _log.WarnFormat("Unable to send service message due to null process shelf reference, service = {0}, message type = {1}",
                                _name, typeof(T).ToShortTypeName());
                return;
            }

            try
            {
                _reference.Send(message);
            }
            catch (AppDomainUnloadedException ex)
            {
                _log.ErrorFormat("[Shelf:{0}] Failed to send to Shelf, AppDomain was unloaded. See next log message for details.", _name);
                _log.Error("See inner exception", ex);

                _publish.Send(new ServiceUnloaded(_name));
            }
        }
    }
}