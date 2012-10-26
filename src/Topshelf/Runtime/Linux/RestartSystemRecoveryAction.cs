namespace Topshelf.Runtime.Linux
{
    public class RestartSystemRecoveryAction
        : ServiceRecoveryAction
    {
        readonly string _message;

        public RestartSystemRecoveryAction(int delayInMinutes, string message)
            : base(delayInMinutes)
        {
            _message = message;
        }

        public override RecoveryAction GetAction()
        {
            return RecoveryAction.RestartSystem;
        }
    }
}