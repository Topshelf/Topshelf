// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Topshelf.HostConfigurators
{
    using System;

    public interface HostConfigurator
    {
        /// <summary>
        ///   Specifies the name of the service as it should be displayed in the service control manager
        /// </summary>
        /// <param name="name"> </param>
        void SetDisplayName(string name);

        /// <summary>
        ///   Specifies the name of the service as it is registered in the service control manager
        /// </summary>
        /// <param name="name"> </param>
        void SetServiceName(string name);

        /// <summary>
        ///   Specifies the description of the service that is displayed in the service control manager
        /// </summary>
        /// <param name="description"> </param>
        void SetDescription(string description);

        /// <summary>
        ///   Specifies the service instance name that should be used when the service is registered
        /// </summary>
        /// <param name="instanceName"> </param>
        void SetInstanceName(string instanceName);

        /// <summary>
        /// Sets the amount of time to wait for the service to start before timing out. Default is 10 seconds.
        /// </summary>
        /// <param name="startTimeOut"></param>
        void SetStartTimeout(TimeSpan startTimeOut);

        /// <summary>
        /// Sets the amount of time to wait for the service to stop before timing out. Default is 10 seconds.
        /// </summary>
        /// <param name="stopTimeOut"></param>
        void SetStopTimeout(TimeSpan stopTimeOut);

        /// <summary>
        /// Enable pause and continue support for the service (default is disabled)
        /// </summary>
        void EnablePauseAndContinue();

        /// <summary>
        /// Enable support for service shutdown (signaled by the host OS)
        /// </summary>
        void EnableShutdown();

        /// <summary>
        /// Enabled support for the session changed event
        /// </summary>
        void EnableSessionChanged();

        /// <summary>
        ///   Specifies the builder factory to use when the service is invoked
        /// </summary>
        /// <param name="hostBuilderFactory"> </param>
        void UseHostBuilder(HostBuilderFactory hostBuilderFactory);

        /// <summary>
        ///   Sets the service builder to use for creating the service
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="serviceBuilderFactory"> </param>
        void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory);

        /// <summary>
        ///   Sets the environment builder to use for creating the service (defaults to Windows)
        /// </summary>
        /// <param name="environmentBuilderFactory"> </param>
        void UseEnvironmentBuilder(EnvironmentBuilderFactory environmentBuilderFactory);

        /// <summary>
        ///   Adds a a configurator for the host builder to the configurator
        /// </summary>
        /// <param name="configurator"> </param>
        void AddConfigurator(HostBuilderConfigurator configurator);

        /// <summary>
        /// Parses the command line options and applies them to the host configurator
        /// </summary>
        void ApplyCommandLine();

        /// <summary>
        /// Parses the command line options from the specified command line and applies them to the host configurator
        /// </summary>
        /// <param name="commandLine"></param>
        void ApplyCommandLine(string commandLine);

        /// <summary>
        /// Adds a command line switch (--name) that can be either true or false. Switches are CASE SeNsITiVe
        /// </summary>
        /// <param name="name">The name of the switch, as it will appear on the command line</param>
        void AddCommandLineSwitch(string name, Action<bool> callback);

        /// <summary>
        /// Adds a command line definition (-name:value) that can be specified. the name is case sensitive. If the 
        /// definition 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddCommandLineDefinition(string name, Action<string> callback);
    }
}