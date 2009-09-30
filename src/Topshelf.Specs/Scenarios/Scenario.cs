namespace Topshelf.Specs.Scenarios
{
    //command line scenarios
    public interface Scenario
    {
        void Execute(ScenarioContext cxt);
    }
}