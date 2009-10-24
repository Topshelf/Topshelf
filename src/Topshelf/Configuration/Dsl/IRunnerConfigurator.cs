// Copyright 2007-2008 The Apache Software Foundation.
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
    using System.Windows.Forms;
    using Model;

    public interface IRunnerConfigurator :
        IDisposable
    {
        /// <summary>
        /// Configures a service using the specified configuration action or set of configuration actions.
        /// </summary>
        /// <typeparam name="TService">The type of the service that will be configured.</typeparam>
        /// <param name="action">The configuration action or set of configuration actions that will be performed.</param>
        void ConfigureService<TService>(string name, Action<IServiceConfigurator<TService>> action) where TService : class;

        /// <summary>
        /// Configures an isolated service using the specified configuration action or set of configuration actions.
        /// </summary>
        /// <typeparam name="TService">The type of the isolated service that will be configured.</typeparam>
        /// <param name="action">The configuration action or set of configuration actions that will be performed.</param>
        void ConfigureServiceInIsolation<TService>(string name, Action<IIsolatedServiceConfigurator<TService>> action) where TService : class;

        /// <summary>
        /// Sets the display name of the service within the Service Control Manager.
        /// </summary>
        /// <param name="displayName">The display name of the service.</param>
        void SetDisplayName(string displayName);

        /// <summary>
        /// Sets the name of the service.
        /// </summary>
        /// <remarks>
        /// This is the name of the service that will be used when starting or stopping the service from the
        /// commandline.
        /// </remarks>
        /// <example>
        /// <para>For the following configuration:</para>
        /// <code><![CDATA[IRunConfiguration config = RunnerConfiguration.New(c =>
        /// {
        ///		c.SetDisplayName("MyService");
        ///	});
        /// ]]>
        /// </code>
        /// <para>the service will be started with the following:</para>
        /// <code>
        /// net start MyService
        /// </code>
        /// </example>
        /// <param name="serviceName">The name of the service.</param>
        void SetServiceName(string serviceName);

        /// <summary>
        /// Sets the description of the service as it will appear in the Service Control Manager.
        /// </summary>
        /// <param name="description">The description of the service.</param>
        void SetDescription(string description);

        /// <summary>
        /// We set the service to start automatically by default. This sets the service to manual instead.
        /// </summary>
        void DoNotStartAutomatically();

        /// <summary>
        /// The application will run with the Local System credentials.
        /// </summary>
        void RunAsLocalSystem();

        /// <summary>
        /// The application will run with the specified credentials.
        /// </summary>
        /// <remarks>
        /// If the application is a Windows service, then ensure that the user account has sufficient
        /// privileges to install and run services.
        /// </remarks>
        /// <param name="username">The name of the user that the application will run as.</param>
        /// <param name="password">The password of the user that the application will run as.</param>
        void RunAs(string username, string password);

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
        /// Registers a dependency on a named Windows service that must start before this service.
        /// </summary>
        /// <param name="serviceName">The name of the service that must start before this service.</param>
        void DependsOn(string serviceName);

        /// <summary>
        /// Registers a dependency on the Microsoft Message Queue service.
        /// </summary>
        void DependencyOnMsmq();

        /// <summary>
        /// Registers a dependency on the Microsoft SQL Server service.
        /// </summary>
        void DependencyOnMsSql();

        /// <summary>
        /// Registers a dependency on the Internet Information Server service.
        /// </summary>
        void DependencyOnIis();
		
        /// <summary>
        /// Registers a dependency on the Event Log service.
        /// </summary>
        void DependencyOnEventLog();

        /// <summary>
        /// By default we use a console/winservice host. By executing this method you state you prefer the winformhost.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void UseWinFormHost<T>() where T : Form;

        /// <summary>
        /// Defines an action or a set of actions to perform before starting the services
        /// </summary>
        /// <param name="action"></param>
        void BeforeStartingServices(Action<IServiceCoordinator> action);

        /// <summary>
        /// Defines an action or a set of actions to perform before the service starts.
        /// </summary>
        /// <remarks>
        /// This is the best place to set up, for example, any IoC containers.
        /// </remarks>
        /// <param name="action">The action or actions that will be performed before the service starts.</param>
        void BeforeStartingTheHost(Action<IServiceCoordinator> action);

        /// <summary>
        /// Defines an action or a set of actions to perform after the service stops.
        /// </summary>
        /// <param name="action">The action or actions that will be performed after the service stops.</param>
        void AfterStoppingTheHost(Action<IServiceCoordinator> action);
    }
}