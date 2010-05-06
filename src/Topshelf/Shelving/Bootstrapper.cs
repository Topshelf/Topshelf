namespace Topshelf.Shelving
{
    public interface Bootstrapper
    {
        //how to set up the required consumers?
        //use the same DSL, so look for 'RunnerConfigurator'?
        object InitializeHostedService();
    }
}