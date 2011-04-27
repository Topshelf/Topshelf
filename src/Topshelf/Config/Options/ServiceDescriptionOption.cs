namespace Topshelf.Options
{
    using HostConfigurators;


    public class ServiceDescriptionOption : Option
    {
        string _description;

        public ServiceDescriptionOption(string description)
        {
            _description = description;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetDescription(_description);
        }
    }
}