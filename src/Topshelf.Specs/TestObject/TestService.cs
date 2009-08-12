namespace Topshelf.Specs.Configuration
{
    using System;

    public class TestService : MarshalByRefObject
    {
        public bool Started;
        public bool Stopped;
        public bool Paused;
        public bool HasBeenContinued;
        public bool HasBeenStarted;

        public TestService()
        {
            Started = false;
            Stopped = false;
            Paused = false;
            HasBeenContinued = false;
            HasBeenStarted = false;
        }

        public void Start()
        {
            HasBeenStarted = true;
            Started = true;
            Stopped = false;
        }

        public void Stop()
        {
            Started = false;
            Stopped = true;
        }

        public void Pause()
        {
            Paused = true;
            Stopped = false;
            Started = false;
        }

        public void Continue()
        {
            Paused = false;
            Stopped = false;
            Started = true;
            HasBeenContinued = true;
        }
    }

    public class TestService2 : TestService
    {
        
    }
}