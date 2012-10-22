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
namespace Topshelf.Supervise
{
    using System;
    using System.IO;

    /// <summary>
    /// Monitors the service directory for a down file (.down) and stops the service
    /// (and prevents it from being started) if the file exists. If the file is present,
    /// the first line of the file is read and used as the reason for the service being 
    /// down.
    /// </summary>
    public class DownFileServiceAvailability :
        ServiceAvailability
    {
        readonly ServiceAvailabilityHost _host;
        string _downFilename;

        public DownFileServiceAvailability(ServiceAvailabilityHost host)
        {
            _host = host;
            _downFilename = BuildDownFilename();
        }

        public bool CanStart(out string reason)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _downFilename = Path.Combine(baseDirectory, ".down");

            if (File.Exists(_downFilename))
            {
                reason = ".down file present - this should read from the file to display the real reason";
                return false;
            }

            reason = null;
            return true;
        }

        static string BuildDownFilename()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, ".down");
        }
    }
}