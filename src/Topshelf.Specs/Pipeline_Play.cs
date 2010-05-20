using System;
using Magnum.Pipeline.Segments;
using Magnum.Pipeline;
using Magnum.Pipeline.Visitors;

namespace Topshelf.Specs
{
    using System.Collections.Generic;
    using Messages;
    using Model;

    public class Pipeline_Play
    {
        public void bob()
        {
            var pipe = PipeSegment.New();
            var ss = pipe.NewSubscriptionScope();

            ss.Subscribe<ServiceMessage>(o => Console.WriteLine("LOG: {0}", o));
            ss.Subscribe<StartService>(s => Console.WriteLine(s.ServiceId));
            ss.Subscribe<StopService>(s => Console.WriteLine(s.ServiceId));
            ss.Subscribe<PauseService>(s => Console.WriteLine(s.ServiceId));
            ss.Subscribe<ContinueService>(s => Console.WriteLine(s.ServiceId));

            pipe.Send(new StartService { ServiceId = "Service1" });

            new TracePipeVisitor().Trace(pipe);
        }

    }

    public class Coordinator
    {
        private readonly Dictionary<string, IServiceController> _controllers = new Dictionary<string, IServiceController>();

        public void StartService(StartService cmd)
        {
            _controllers[cmd.ServiceId].Start();
        }
    }

    //these have a coordinater
    //once hosted they fire a command to the coordinator
    public class ConsoleHost { }
    public class ServiceHost { }

    public class ServiceInstaller { }
}