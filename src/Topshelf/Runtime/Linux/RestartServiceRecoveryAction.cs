namespace Topshelf.Runtime.Linux
{
    public class RestartServiceRecoveryAction
        : ServiceRecoveryAction
    {
        public RestartServiceRecoveryAction(int delayInMinutes) : base(delayInMinutes)
        {
        }

        public override RecoveryAction GetAction()
        {
            return RecoveryAction.RestartService;
        }
    }
}