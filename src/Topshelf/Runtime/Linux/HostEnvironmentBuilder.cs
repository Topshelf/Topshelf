using Topshelf.Builders;

namespace Topshelf.Runtime.Linux
{
    public class HostEnvironmentBuilder
        : EnvironmentBuilder
    {
        public HostEnvironment Build()
        {
            return new LinuxHostEnvironment();
        }
    }
}