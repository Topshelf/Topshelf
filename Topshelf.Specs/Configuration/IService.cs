namespace Topshelf.Specs.Configuration
{
    using System;

    public interface IService
    {
        Type ServiceType { get; }
        string Name { get; }
        ServiceState State { get; }
        void Start();
        void Stop();
        void Pause();
        void Continue();
    }
}