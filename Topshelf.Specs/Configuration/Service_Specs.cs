namespace Topshelf.Specs.Configuration
{
    using MbUnit.Framework;
    using Microsoft.Practices.ServiceLocation;
    using Rhino.Mocks;
    using Topshelf.Configuration;

    [TestFixture]
    public class Service_Specs
    {
        private IService _service;
        private TestService _srv;
        private bool _wasPaused;
        private bool _wasContinued;

        [SetUp]
        public void EstablishContext()
        {
            _srv = new TestService();
            IServiceLocator sl = MockRepository.GenerateStub<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(()=>sl);
            sl.Stub(x => x.GetInstance<TestService>()).Return(_srv);

            ServiceConfigurator<TestService> c = new ServiceConfigurator<TestService>();
            c.WithName("my_service");
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());
            c.WhenPaused(s => { _wasPaused = true; });
            c.WhenContinued(s => { _wasContinued = true; });
            _service = c.Create();
        }

        [Test]
        public void Should_stop()
        {
            _service.Stop();

            _service.State
                .ShouldEqual(ServiceState.Stopped);

            _srv.Stopped
                .ShouldBeTrue();
        }

        [Test]
        public void Should_start()
        {
            _service.Start();

            _service.State
                .ShouldEqual(ServiceState.Started);

            _srv.Stopped
                .ShouldBeFalse();
            _srv.Started
                .ShouldBeTrue();
        }

        [Test]
        public void Should_pause()
        {
            _service.Pause();

            _service.State
                .ShouldEqual(ServiceState.Paused);

            _wasPaused
                .ShouldBeTrue();
        }

        [Test]
        public void Should_continue()
        {
            _service.Continue();

            _service.State
                .ShouldEqual(ServiceState.Started);
            _wasContinued
                .ShouldBeTrue();
        }

        [Test]
        public void Should_expose_contained_type()
        {
            _service.ServiceType
                .ShouldEqual(typeof(TestService));
        }

        //TODO: state transition tests
    }
}