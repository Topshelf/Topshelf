namespace Topshelf.Runtime.Linux
{
    public class RunProgramRecoveryAction
        : ServiceRecoveryAction
    {
        readonly string _command;

        public RunProgramRecoveryAction(int delayInMinutes, string command) : base(delayInMinutes)
        {
            _command = command;
        }

        public override RecoveryAction GetAction()
        {
            return RecoveryAction.RunCommand;
        }
    }
}