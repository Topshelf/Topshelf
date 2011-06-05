namespace Topshelf.Options
{
    using HostConfigurators;


    public class ServiceNameOption : Option
    {
        string _serviceName;

        public ServiceNameOption(string name)
        {
            _serviceName = name;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetServiceName(_serviceName);
        }
    }
}