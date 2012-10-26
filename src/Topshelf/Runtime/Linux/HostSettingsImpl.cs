namespace Topshelf.Runtime.Linux
{
    using System;

    [Serializable]
    public class HostSettingsImpl :
        HostSettings
    {
        public const string InstanceSeparator = "$";
        string _description;
        string _displayName;

        /// <summary>
        ///   Creates a new WindowsServiceDescription using empty strings for the properties. The class is required to have names by the consumers.
        /// </summary>
        public HostSettingsImpl()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        ///   Creates a new WindowsServiceDescription instance using the passed parameters.
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="instanceName"> </param>
        public HostSettingsImpl(string name, string instanceName)
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

                if (!string.IsNullOrEmpty(InstanceName))
                    return string.Format("{0} (Instance: {1})", displayName, InstanceName);

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

        public bool CanShutdown { get; set; }
    }
}