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
		Restart
	}
}

