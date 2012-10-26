using System;

namespace Topshelf
{
	public abstract class ServiceRecoveryAction
    {
        protected ServiceRecoveryAction(int delay)
        {
            Delay = delay;
        }

        public int Delay { get; private set; }

        public abstract RecoveryAction GetAction();
    }
	
	// have a look at what upstart supports
	
	public enum RecoveryAction
	{ 
        None = 0,
        RestartService = 1,
        RestartSystem = 2,
        RunCommand = 3
	}
}

