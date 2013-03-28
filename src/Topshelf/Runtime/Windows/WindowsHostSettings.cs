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
namespace Topshelf.Runtime.Windows
{
    using System;

    [Serializable]
    public class WindowsHostSettings :
        HostSettings
    {
        public const string InstanceSeparator = "$";
        string _description;
        string _displayName;

        /// <summary>
        ///   Creates a new WindowsServiceDescription using empty strings for the properties. The class is required to have names by the consumers.
        /// </summary>
        public WindowsHostSettings()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        ///   Creates a new WindowsServiceDescription instance using the passed parameters.
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="instanceName"> </param>
        public WindowsHostSettings(string name, string instanceName)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (instanceName == null)
                throw new ArgumentNullException("instanceName");

            Name = name;
            InstanceName = instanceName;

            _displayName = "";
            _description = "";
        }

        public string Name { get; set; }

        public string DisplayName
        {
            get
            {
                string displayName = string.IsNullOrEmpty(_displayName)
                                         ? Name
                                         : _displayName;

                var instance = string.Format(" (Instance: {0})", InstanceName);
                if (!string.IsNullOrEmpty(InstanceName) && !displayName.EndsWith(instance))
                    return displayName + instance;

                return displayName;
            }
            set { _displayName = value; }
        }


        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(_description)
                           ? DisplayName
                           : _description;
            }
            set { _description = value; }
        }


        public string InstanceName { get; set; }

        public string ServiceName
        {
            get
            {
                return string.IsNullOrEmpty(InstanceName)
                           ? Name
                           : Name + InstanceSeparator + InstanceName;
            }
        }

        public bool CanPauseAndContinue { get; set; }

        public bool CanHandleSessionChangeEvent { get; set; }

        public bool CanShutdown { get; set; }
    }
}