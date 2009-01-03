namespace Topshelf.Specs.Configuration
{
    public class TestService
    {
        public bool Started;
        public bool Stopped;

        public void Start()
        {
            Started = true;
            Stopped = false;
        }

        public void Stop()
        {
            Started = false;
            Stopped = true;
        }
    }

    public class TestService2 : TestService
    {
        
    }
}