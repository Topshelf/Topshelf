namespace Topshelf.Options
{
    using HostConfigurators;


    public class InteractiveOption : Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.RunAsPrompt();
        }
    }
}