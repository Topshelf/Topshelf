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
	using Magnum.Channels;
	using Magnum.Extensions;
	using Messages;
	using Model;


	public interface ServiceFactory
	{
		
	}

	public class ShelfServiceFactory :
		ServiceFactory
	{
		
	}

	public class CurrentAppDomainServiceFactory :
		ServiceFactory
	{
		
	}

	public class ShelfService :
		ServiceStateMachine<ShelfService, CreateShelfService, ShelfCreated>
	{
		static readonly ILog _log = LogManager.GetLogger(typeof(ShelfService));

		AssemblyName[] _assemblyNames;
		Type _bootstrapperType;
		ShelfReference _reference;
		ShelfType _shelfType;

		public ShelfService(string name, UntypedChannel eventChannel)
			: base(name, eventChannel)
		{
		}


		void Send<T>(T message)
		{
			if (_reference == null || _reference.ShelfChannel == null)
			{
				_log.WarnFormat("Unable to send service message due to null channel, service = {0}, message type = {1}",
				                Name, typeof(T).ToShortTypeName());
				return;
			}

			_reference.ShelfChannel.Send(message);
		}

		protected override void Create(CreateShelfService message)
		{
			_log.Debug("Creating shelf service: " + message.ServiceName);

			Name = message.ServiceName;

			_shelfType = message.ShelfType;
			_bootstrapperType = message.BootstrapperType;
			_assemblyNames = message.AssemblyNames;

			Create();
		}

		protected override void Create()
		{
			_reference = new ShelfReference(Name, _shelfType);

			if (_assemblyNames != null)
				_assemblyNames.Each(_reference.LoadAssembly);

			if (_bootstrapperType != null)
				_reference.Create(_bootstrapperType);
			else
				_reference.Create();
		}

		protected override void ServiceCreated(ShelfCreated message)
		{
			_reference.CreateShelfChannel(message.Address, message.PipeName);
		}

		protected override void Start()
		{
			_log.Debug("Starting service: " + Name);
			Send(new StartService
				{
					ServiceName = Name,
				});
		}

		protected override void Stop(StopService message)
		{
			Send(message);
		}

		protected override void Unload()
		{
			if (_reference != null)
			{
				_reference.Dispose();
				_reference = null;
			}
		}
	}
}