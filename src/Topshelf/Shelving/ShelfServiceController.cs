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


	public class ShelfServiceController :
		ServiceStateMachine
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Shelving.ShelfServiceController");

		readonly AssemblyName[] _assemblyNames;
		readonly Type _bootstrapperType;
		readonly ShelfType _shelfType;

		int _restartCount;
		int _restartLimit;

		ShelfReference _reference;

		public ShelfServiceController(string name, ServiceChannel eventChannel, ShelfType shelfType, Type bootstrapperType,
		                              AssemblyName[] assemblyNames)
			: base(name, eventChannel)
		{
			_shelfType = shelfType;
			_bootstrapperType = bootstrapperType;
			_assemblyNames = assemblyNames;

			_restartLimit = 5;
		}


		void Send<T>(T message)
		{
			if (_reference == null)
			{
				_log.WarnFormat("Unable to send service message due to null shelf reference, service = {0}, message type = {1}",
				                Name, typeof(T).ToShortTypeName());
				return;
			}

			try
			{
				_reference.Send(message);
			}
			catch (AppDomainUnloadedException ex)
			{
				_log.ErrorFormat("[Shelf:{0}] Failed to send to Shelf, AppDomain was unloaded", Name);

				Publish<ServiceUnloaded>();
			}
		}


		protected override void Create(CreateService message)
		{
			Create();
		}

		protected override void Create()
		{
			_log.DebugFormat("[Shelf:{0}] Creating shelf service", Name);

			_reference = new ShelfReference(Name, _shelfType);

			if (_assemblyNames != null)
				_assemblyNames.Each(_reference.LoadAssembly);

			if (_bootstrapperType != null)
				_reference.Create(_bootstrapperType);
			else
				_reference.Create();
		}

		protected override void ServiceCreated(ServiceCreated message)
		{
			_log.DebugFormat("[Shelf:{0}] Shelf created at {1} ({2})", Name, message.Address, message.PipeName);

			_reference.CreateShelfChannel(message.Address, message.PipeName);
		}

		protected override void ServiceFaulted(ServiceFault message)
		{
			_log.ErrorFormat("[Shelf:{0}] Shelf Service Faulted: {1}", Name, message.ExceptionDetail);

			try
			{
				_reference.Dispose();
			}
			catch (Exception ex)
			{
				_log.Error("[Shelf:{0}] Exception disposing of reference" + Name, ex);
			}
			finally
			{
				_reference = null;
			}

			_log.DebugFormat("[Shelf:{0}] Shelf Reference Discarded", Name);

			RestartService();
		}

		void RestartService()
		{
			if (_restartCount >= _restartLimit)
			{
				_log.DebugFormat("[Shelf:{0}] Restart Limit Reached ({1})", Name, _restartCount);
				return;
			}

			_restartCount++;

			Publish(new RestartService(Name));
		}

		protected override void Start()
		{
			_log.DebugFormat("[Shelf:{0}] Start", Name);

			Send(new StartService(Name));
		}

		protected override void Stop()
		{
			_log.DebugFormat("[Shelf:{0}] Stop", Name);

			Send(new StopService(Name));
		}

		protected override void Pause()
		{
			_log.DebugFormat("[Shelf:{0}] Pause", Name);

			Send(new PauseService(Name));
		}

		protected override void Continue()
		{
			_log.DebugFormat("[Shelf:{0}] Continue", Name);

			Send(new ContinueService(Name));
		}

		protected override void Unload()
		{
			_log.DebugFormat("[Shelf:{0}] {1}", Name, "Unloading");
			Publish<ServiceUnloading>();

			if (_reference != null)
			{
				Send(new UnloadService(Name));

				_reference.Dispose();
				_reference = null;
			}
			else
			{
				Publish<ServiceUnloaded>();
				_log.WarnFormat("[Shelf:{0}] {1}", Name, "Was already unloaded");
			}
		}
	}
}