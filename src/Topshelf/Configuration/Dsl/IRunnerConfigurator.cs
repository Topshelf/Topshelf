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
namespace Topshelf.Configuration.Dsl
{
	using System;
	using HostConfigurators;
	using Model;


	public interface IRunnerConfigurator :
		HostConfigurator
	{
		/// <summary>
		/// Configures a service using the specified configuration action or set of configuration actions.
		/// </summary>
		/// <typeparam name="TService">The type of the service that will be configured.</typeparam>
		/// <param name="action">The configuration action or set of configuration actions that will be performed.</param>
		void ConfigureService<TService>(Action<IServiceConfigurator<TService>> action) where TService : class;


		/// <summary>
		/// We set the service to start automatically by default. This sets the service to manual instead.
		/// </summary>
		void DoNotStartAutomatically();

		/// <summary>
		/// The application will run with the Local System credentials, with the ability to interact with the desktop.
		/// </summary>
		void RunAsFromInteractive();

		/// <summary>
		/// The application will run with the credentials specified in the commandline arguments.
		/// </summary>
		/// <example>
		/// The commandline arguments should be in the format:
		/// <code><![CDATA[MyApplication.exe /credentials:username#password]]>
		/// </code>
		/// This means that <c>#</c> will not be a valid character to use in the password.
		/// </example>
		void RunAsFromCommandLine();

		/// <summary>
		/// Sets the length of time the system will wait for events to complete, such as starting or stopping
		/// services.
		/// </summary>
		/// <param name="timeout">The wait timeout</param>
		void SetEventTimeout(TimeSpan timeout);

		/// <summary>
		/// Sets up the service recovery options exposed to the windows serivces. 
		/// </summary>
		/// <param name="recoveryConfigurator">An action configuring the recovery settings</param>
		void UseServiceRecovery(Action<IServiceRecoveryConfigurator> recoveryConfigurator);

		void EnableDashboard();
    }
}