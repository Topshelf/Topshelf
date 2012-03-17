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
namespace Topshelf.Builders
{
	using System;

    /// <summary>
    /// The base interface for building out configuration.
    /// </summary>
	public interface HostBuilder
	{
        /// <summary>
        /// Exposes the 'Service Description'
        /// </summary>
		ServiceDescription Description { get; }

        /// <summary>
        /// Builds the Host to be run.
        /// </summary>
        /// <returns></returns>
		Host Build();

        /// <summary>
        /// Should this 'builder' be run?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
		void Match<T>(Action<T> callback)
			where T : class, HostBuilder;
	}
}