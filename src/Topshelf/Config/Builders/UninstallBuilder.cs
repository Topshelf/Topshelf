// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceProcess;
	using Hosts;
	using Internal;


	public class UninstallBuilder :
		HostBuilder
	{
		readonly IList<string> _dependencies;
		readonly ServiceDescription _description;
		readonly IList<Action> _postActions;
		readonly IList<Action> _preActions;
	    readonly Credentials _credentials;
	    readonly ServiceStartMode _startMode;
        readonly bool _delayedAutoStart;
	    private bool _sudo;

		public UninstallBuilder(ServiceDescription description)
		{
			_preActions = new List<Action>();
			_postActions = new List<Action>();
			_dependencies = new List<string>();
			_startMode = ServiceStartMode.Automatic;
			_credentials = new Credentials("", "", ServiceAccount.LocalSystem);
		    _delayedAutoStart = false;
			_description = description;
		}

		public ServiceDescription Description
		{
			get { return _description; }
		}

		public Host Build()
		{
			return new UninstallHost(_description, _startMode, _dependencies.ToArray(), _credentials, _preActions, _postActions,
			                         _sudo, _delayedAutoStart);
		}

		public void Match<T>([NotNull] Action<T> callback)
			where T : class, HostBuilder
		{
			if (callback == null)
				throw new ArgumentNullException("callback");

			if (typeof(T).IsAssignableFrom(GetType()))
				callback(this as T);
		}

		public void Sudo()
		{
			_sudo = true;
		}

		public void BeforeUninstall(Action callback)
		{
			_preActions.Add(callback);
		}

		public void AfterUninstall(Action callback)
		{
			_postActions.Add(callback);
		}
	}
}