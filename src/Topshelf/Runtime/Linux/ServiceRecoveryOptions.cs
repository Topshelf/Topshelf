using System;

namespace Topshelf.Runtime.Linux
{
    using System.Collections.Generic;

    public class ServiceRecoveryOptions
    {
        readonly IList<ServiceRecoveryAction> _actions;

        public ServiceRecoveryOptions()
        {
            _actions = new List<ServiceRecoveryAction>();
        }

        public int ResetPeriod { get; set; }

        public IEnumerable<ServiceRecoveryAction> Actions
        {
            get { return _actions; }
        }

        public void AddAction(ServiceRecoveryAction serviceRecoveryAction)
        {
            _actions.Add(serviceRecoveryAction);
		}
	}
}