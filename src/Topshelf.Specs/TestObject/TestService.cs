namespace Topshelf.Specs.TestObject
{
    using System;

    public class TestService : MarshalByRefObject
    {
        public bool Started;
        public bool Stopped;
        public bool Paused;
        public bool HasBeenContinued;
        public bool HasBeenStarted;
        public Action StartAction;

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
            if (StartAction != null)
            {
                StartAction();
            }

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
}