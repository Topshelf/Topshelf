namespace Topshelf.Specs.Configuration
{
    using Internal;
    using MbUnit.Framework;
    using Microsoft.Practices.ServiceLocation;
    using Rhino.Mocks;
    using Topshelf.Configuration;

    [TestFixture]
    public class A_service_should_control_its_subservices
    {
        private ServiceCoordinator _serviceCoordinator;
        private TestService _service;
        private TestService2 _service2;

        [SetUp]
        public void EstablishContext()
        {
            _service = new TestService();
            _service2 = new TestService2();

            IServiceLocator sl = MockRepository.GenerateStub<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => sl);
            sl.Stub(x => x.GetInstance<TestService>()).Return(_service).Repeat.Any();
            sl.Stub(x => x.GetInstance<TestService2>()).Return(_service2).Repeat.Any();

            _serviceCoordinator = (ServiceCoordinator)RunnerConfigurator.New(x =>
            {
                x.ConfigureService<TestService>(c =>
                {
                    c.WithName("my_service");
                    c.WhenStarted(s => s.Start());
                    c.WhenStopped(s => s.Stop());
                    c.WhenPaused(s => { });
                    c.WhenContinued(s => { });
                });
                x.ConfigureService<TestService2>(c =>
                {
                    c.WithName("my_service2");
                    c.WhenStarted(s => s.Start());
                    c.WhenStopped(s => s.Stop());
                    c.WhenPaused(s => { });
                    c.WhenContinued(s => { });
                });
            });
        }

        [TearDown]
        public void TeardownContext()
        {
            _serviceCoordinator = null;
            _service = null;
            _service2 = null;
        }

        [Test]
        public void Should_start_all_services()
        {
            _serviceCoordinator.Start();

            _service.Started
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }

        [Test]
        public void Should_stop_all_services()
        {
            _serviceCoordinator.Start();
            _serviceCoordinator.Stop();

            _service.Stopped
                .ShouldBeTrue();
            _service2.Stopped
                .ShouldBeTrue();
        }

        [Test]
        public void Start_individual_services()
        {
            _service.Started
                .ShouldBeFalse();
            _service2.Started
                .ShouldBeFalse();

            _serviceCoordinator.StartService("my_service");

            _service.Started
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeFalse();
        }

        [Test]
        public void Stop_individual_services()
        {
            _serviceCoordinator.Start();

            _serviceCoordinator.StopService("my_service");

            _service.Started
                .ShouldBeFalse();
            _service2.Started
                .ShouldBeTrue();

        }
    }
}