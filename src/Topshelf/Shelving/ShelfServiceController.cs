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
	using System.Reflection;
	using log4net;
	using Magnum.Extensions;
	using Messages;
	using Model;
	using Stact;
	using Stact.Workflow;


	public class ShelfServiceController :
		IServiceController
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Shelving.ShelfServiceController");

		readonly AssemblyName[] _assemblyNames;
		readonly Type _bootstrapperType;
		readonly UntypedChannel _eventChannel;
		readonly Inbox _inbox;
		readonly string _name;
		readonly ShelfType _shelfType;
		bool _disposed;
		ShelfReference _reference;

		int _restartCount;
		int _restartLimit;

		public ShelfServiceController(Inbox inbox, string name, UntypedChannel eventChannel, ShelfType shelfType, Type bootstrapperType,
		                              AssemblyName[] assemblyNames)
		{
			_inbox = inbox;
			_name = name;
			_eventChannel = eventChannel;
			_shelfType = shelfType;
			_bootstrapperType = bootstrapperType;
			_assemblyNames = assemblyNames;

			_restartLimit = 5;
		}


		public Type ServiceType
		{
			get { return typeof(Shelf); }
		}

		public State CurrentState { get; set; }

		public string Name
		{
			get { return _name; }
		}

		public void Create()
		{
			_log.DebugFormat("[Shelf:{0}] Creating shelf service", _name);

			_reference = new ShelfReference(_name, _shelfType);

			if (_assemblyNames != null)
				_assemblyNames.Each(_reference.LoadAssembly);

			if (_bootstrapperType != null)
				_reference.Create(_bootstrapperType);
			else
				_reference.Create();
		}

		public void Start()
		{
			_log.DebugFormat("[Shelf:{0}] Start", _name);

			Send(new StartService(_name));
		}

		public void Stop()
		{
			_log.DebugFormat("[Shelf:{0}] Stop", _name);

			Send(new StopService(_name));
		}

		public void Pause()
		{
			_log.DebugFormat("[Shelf:{0}] Pause", _name);

			Send(new PauseService(_name));
		}

		public void Continue()
		{
			_log.DebugFormat("[Shelf:{0}] Continue", _name);

			Send(new ContinueService(_name));
		}

		public void Unload()
		{
			_log.DebugFormat("[Shelf:{0}] {1}", _name, "Unloading");
			_eventChannel.Send(new ServiceUnloading(_name));

			if (_reference != null)
			{
				Send(new UnloadService(_name));

				_reference.Dispose();
				_reference = null;
			}
			else
			{
				_eventChannel.Send(new ServiceUnloaded(_name));
				_log.WarnFormat("[Shelf:{0}] {1}", _name, "Was already unloaded");
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Send<T>(T message)
		{
			if (_reference == null)
			{
				_log.WarnFormat("Unable to send service message due to null shelf reference, service = {0}, message type = {1}",
				                _name, typeof(T).ToShortTypeName());
				return;
			}

			try
			{
				_reference.Send(message);
			}
			catch (AppDomainUnloadedException ex)
			{
				_log.ErrorFormat("[Shelf:{0}] Failed to send to Shelf, AppDomain was unloaded", _name);

				_eventChannel.Send(new ServiceUnloaded(_name));
			}
		}

		void Created(ServiceCreated message)
		{
			_log.DebugFormat("[Shelf:{0}] Shelf created at {1} ({2})", _name, message.Address, message.PipeName);

			_reference.CreateShelfChannel(message.Address, message.PipeName);
		}

		void Faulted(ServiceFault message)
		{
			_log.ErrorFormat("[Shelf:{0}] Shelf Service Faulted: {1}", _name, message.ExceptionDetail);

			try
			{
				_reference.Dispose();
			}
			catch (Exception ex)
			{
				_log.Error("[Shelf:{0}] Exception disposing of reference" + _name, ex);
			}
			finally
			{
				_reference = null;
			}

			_log.DebugFormat("[Shelf:{0}] Shelf Reference Discarded", _name);

			RestartService();
		}

		void RestartService()
		{
			if (_restartCount >= _restartLimit)
			{
				_log.DebugFormat("[Shelf:{0}] Restart Limit Reached ({1})", _name, _restartCount);
				return;
			}

			_restartCount++;

			_eventChannel.Send(new RestartService(_name));
		}

		~ShelfServiceController()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_reference != null)
				{
					_reference.Dispose();
					_reference = null;
				}
			}

			_disposed = true;
		}
	}
}