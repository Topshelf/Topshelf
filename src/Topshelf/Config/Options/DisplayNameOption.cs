namespace Topshelf.Options
{
    using HostConfigurators;


    public class DisplayNameOption : Option
    {
        string _name;

        public DisplayNameOption(string name)
        {
            _name = name;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetDisplayName(_name);
        }
    }
}