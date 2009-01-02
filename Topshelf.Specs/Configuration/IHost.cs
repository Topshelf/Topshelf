namespace Topshelf.Specs.Configuration
{
    using System.Collections.Generic;

    public interface IHost
    {
        void Start();
        void Stop();
        void Pause();
        void Continue();

        void StartService(string name);
        void StopService(string name);
        void PauseService(string name);
        void ContinueService(string name);

        //void Install();
        //void Uninstall();
    }
}