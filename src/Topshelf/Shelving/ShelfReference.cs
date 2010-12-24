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
	using System.Configuration;
	using System.IO;
	using System.Reflection;
	using System.Runtime.Remoting;
	using log4net;
	using Magnum.Extensions;
	using Model;


	public class ShelfReference :
		IDisposable
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Shelving.ShelfReference");

		readonly string _serviceName;
		readonly ShelfType _shelfType;
		OutboundChannel _channel;
		bool _disposed;
		AppDomain _domain;
		AppDomainSetup _domainSettings;
		ObjectHandle _objectHandle;

		public ShelfReference(string serviceName, ShelfType shelfType)
		{
			_serviceName = serviceName;
			_shelfType = shelfType;

			ConfigureAppDomainSettings();

			_domain = AppDomain.CreateDomain(serviceName, null, _domainSettings);
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Send<T>(T message)
		{
			if (_channel == null)
			{
				_log.WarnFormat("Unable to send service message due to null channel, service = {0}, message type = {1}",
				                _serviceName, typeof(T).ToShortTypeName());
				return;
			}

			_channel.Send(message);
		}

		public void LoadAssembly(AssemblyName assemblyName)
		{
			_domain.Load(assemblyName);
		}

		public void Create()
		{
			CreateShelfInstance(null);
		}

		public void Create(Type bootstrapperType)
		{
			_log.DebugFormat("[{0}].BootstrapperType = {1}", _serviceName, bootstrapperType.ToShortTypeName());
			_log.DebugFormat("[{0}].BootstrapperAssembly = {1}", _serviceName, bootstrapperType.Assembly.GetName().Name);
			_log.DebugFormat("[{0}].BootstrapperVersion = {1}", _serviceName, bootstrapperType.Assembly.GetName().Version);

			CreateShelfInstance(bootstrapperType);
		}

		void CreateShelfInstance(params object[] args)
		{
			_log.DebugFormat("Creating Shelf Instance: " + _serviceName);

			Type shelfType = typeof(Shelf);

			_objectHandle = _domain.CreateInstance(shelfType.Assembly.GetName().FullName, shelfType.FullName, true, 0, null,
			                       args ?? new object[] {null},
			                       null, null, null);
		}

		void ConfigureAppDomainSettings()
		{
			_domainSettings = AppDomain.CurrentDomain.SetupInformation;
			if (_shelfType == ShelfType.Internal)
				return;

			_domainSettings.ShadowCopyFiles = "true";

			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			string servicesDirectory = ConfigurationManager.AppSettings["MonitorDirectory"] ?? "Services";

			_domainSettings.ApplicationBase = Path.Combine(baseDirectory, Path.Combine(servicesDirectory, _serviceName));
			_log.DebugFormat("[{0}].ApplicationBase = {1}", _serviceName, _domainSettings.ApplicationBase);

			_domainSettings.ConfigurationFile = Path.Combine(_domainSettings.ApplicationBase, _serviceName + ".config");
			_log.DebugFormat("[{0}].ConfigurationFile = {1}", _serviceName, _domainSettings.ConfigurationFile);
		}

		public void CreateShelfChannel(Uri uri, string pipeName)
		{
			_log.DebugFormat("[{0}] Creating shelf proxy: {1} ({2})", _serviceName, uri, pipeName);

			_channel = new OutboundChannel(uri, pipeName);
		}

		~ShelfReference()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			if (disposing)
			{
				if (_channel != null)
				{
					_channel.Dispose();
					_channel = null;
				}

				try
				{
					_log.DebugFormat("[{0}] Unloading AppDomain", _serviceName);

					_log.InfoFormat("[{0}] AppDomain Unloaded", _serviceName);
				}
				catch (Exception)
				{
					_log.DebugFormat("[{0}] AppDomain was already unloaded", _serviceName);
				}
				finally
				{
					_domain = null;
				}
			}

			_disposed = true;
		}
	}
}