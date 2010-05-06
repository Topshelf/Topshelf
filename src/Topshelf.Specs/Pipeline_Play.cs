using System;
using Magnum.Pipeline.Segments;
using Magnum.Pipeline;
using Magnum.Pipeline.Visitors;

namespace Topshelf.Specs
{
    using System.Collections.Generic;
    using Model;

    public class Pipeline_Play
    {
        public void bob()
        {
            var pipe = PipeSegment.New();
            var ss = pipe.NewSubscriptionScope();
            
            ss.Subscribe<ServiceMessage>(o=>Console.WriteLine("LOG: {0}", o));
            ss.Subscribe<StartService>(s => Console.WriteLine(s.ServiceId));
            ss.Subscribe<StopService>(s => Console.WriteLine(s.ServiceId));
            ss.Subscribe<PauseService>(s => Console.WriteLine(s.ServiceId));
            ss.Subscribe<ContinueService>(s => Console.WriteLine(s.ServiceId));
           
                
            pipe.Send(new StartService(){ServiceId = Guid.NewGuid()});

            new TracePipeVisitor().Trace(pipe);
        }

    }

    public class ServiceMessage
    {
        public Guid ServiceId { get; set; }
        public Guid ConversationId { get; set; }
    }

    //host commands
    public class RunAsConsole {}
    public class Install {} // instance name
    public class Uninstall{} //instance name
    public class RunAsService{} //instance name


    //commands
    public class StartService : ServiceMessage {}
    public class StopService : ServiceMessage {}
    public class ContinueService : ServiceMessage {}
    public class PauseService : ServiceMessage {}
    
    //events
    public class ServiceStarted : ServiceMessage {}
    public class ServiceStopped : ServiceMessage {}
    public class ServicePaused : ServiceMessage {}
    public class ServiceContinued : ServiceMessage {}

    public class Coordinator
    {
        private Dictionary<Guid, IServiceController> _controllers;

        public void StartService(StartService cmd)
        {
            _controllers[cmd.ServiceId].Start();
        }
    }

    //these have a coordinater
    //once hosted they fire a command to the coordinator
    public class ConsoleHost {}
    public class ServiceHost {}

    
    public class ServiceInstaller{}
}