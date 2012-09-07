namespace Topshelf.Supervise.Commands
{
    using System;
    using Runtime;

    public class StartServiceCommand :
        Command
    {
        readonly Guid _compensateId;
        readonly Guid _executeId;

        public StartServiceCommand()
        {
            _executeId = new Guid("2E5B29D5-9C22-4C18-BAA5-FC3E36EBF4D9");
            _compensateId = new Guid("9E9B8C61-2072-4910-9A52-410D887D0286");
        }

        public CommandAudit Execute(CommandTask task)
        {
            var serviceHandle = task.Arguments.Get<ServiceHandle>();
            var hostControl = task.Arguments.Get<HostControl>();

            bool started = serviceHandle.Start(hostControl);

            return new CommandAudit(this, new CommandResult
                {
                    {"started", started}
                });
        }

        public bool Compensate(CommandAudit audit, WorkList workList)
        {
            var started = audit.Get<bool>("started");

            if(started)
            {
                var serviceHandle = audit.Get<ServiceHandle>();
                var hostControl = audit.Get<HostControl>();

                return serviceHandle.Stop(hostControl);
            }

            return true;
        }

        public Guid ExecuteId
        {
            get { return _executeId; }
        }

        public Guid CompensateId
        {
            get { return _compensateId; }
        }
    }
}