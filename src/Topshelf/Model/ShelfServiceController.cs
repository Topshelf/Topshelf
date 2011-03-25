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
namespace Topshelf.Model
{
	using System;
	using System.Reflection;
	using Exceptions;
	using log4net;
	using Magnum.Extensions;
	using Messages;
	using Stact;
	using Stact.Workflow;


	public class ShelfServiceController :
		IServiceController
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Model.ShelfServiceController");

		readonly AssemblyName[] _assemblyNames;
		readonly Type _bootstrapperType;
		readonly Inbox _inbox;
		readonly string _name;
		readonly PublishChannel _publish;
		readonly ShelfType _shelfType;
		bool _disposed;
		ShelfReference _reference;

		public ShelfServiceController(Inbox inbox, string name, IServiceChannel coordinatorChannel, ShelfType shelfType,
		                              Type bootstrapperType, AssemblyName[] assemblyNames)
		{
			_inbox = inbox;
			_name = name;
			_publish = new PublishChannel(coordinatorChannel, inbox);
			_shelfType = shelfType;
			_bootstrapperType = bootstrapperType;
			_assemblyNames = assemblyNames;

			_inbox.Loop(loop =>
				{
					loop.Receive<ShelfCreated>(x =>
						{
							ShelfCreated(x);
							loop.Continue();
						});

					loop.Receive<ServiceUnloaded>(x =>
						{
							ShelfUnloaded(x);
							loop.Continue();
						});
				});
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
			try
			{
				_log.DebugFormat("[Shelf:{0}] Create", _name);

				_reference = new ShelfReference(_name, _shelfType, _publish);

				if (_assemblyNames != null)
					_assemblyNames.Each(_reference.LoadAssembly);

				if (_bootstrapperType != null)
					_reference.Create(_bootstrapperType);
				else
					_reference.Create();
			}
			catch (Exception ex)
			{
				// Henrik: remove this line in favor of the other exception handling mechanism 
				// handling the faulted state, but right now I want the binding information.
				_log.Error("cannot create shelf", ex);

				var buildServiceException = 
					_bootstrapperType == null 
					? new BuildServiceException(_name, ex)
					: new BuildServiceException(_name, _bootstrapperType, ex);

				_publish.Send(new ServiceFault(_name, buildServiceException));
			}
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
			_publish.Send(new ServiceUnloading(_name));

			if (_reference != null)
			{
				Send(new UnloadService(_name));
				_reference.Unload();
			}
			else
			{
				_publish.Send(new ServiceUnloaded(_name));
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
				_log.ErrorFormat("[Shelf:{0}] Failed to send to Shelf, AppDomain was unloaded. See next log message for details.", _name);
				_log.Error("See inner exception", ex);

				_publish.Send(new ServiceUnloaded(_name));
			}
		}

		void ShelfCreated(ShelfCreated message)
		{
			_log.DebugFormat("[Shelf:{0}] Shelf created at {1} ({2})", _name, message.Address, message.PipeName);

			_reference.CreateShelfChannel(message.Address, message.PipeName);
		}

		void ShelfUnloaded(ServiceUnloaded message)
		{
			_reference.Dispose();
			_reference = null;

			_log.DebugFormat("[Shelf:{0}] {1}", _name, "Unloaded");

			_publish.Send(message);
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