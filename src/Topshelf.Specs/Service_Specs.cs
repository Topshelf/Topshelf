namespace Topshelf.Specs.Configuration
{
    using Internal;
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

            ServiceConfigurator<TestService> c = new ServiceConfigurator<TestService>("my_service");
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());
            c.WhenPaused(s => { _wasPaused = true; });
            c.WhenContinued(s => { _wasContinued = true; });
            c.CreateServiceLocator(()=>
                                   {
                                       IServiceLocator sl = MockRepository.GenerateStub<IServiceLocator>();
                                       ServiceLocator.SetLocatorProvider(() => sl);
                                       sl.Stub(x => x.GetInstance<TestService>("my_service")).Return(_srv);
                                       return sl;
                                   });
            _service = c.Create();
            _service.Start();
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
            //_service.Start();

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

	[TestFixture]
	public class SimpleServiceContainerStuff
	{
		[Test]
		public void Should_work()
		{
            var c = new ServiceConfigurator<TestService>("my_service");
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());
            c.CreateServiceLocator(()=>
                                   {
                                       TestService srv = new TestService();
                                       var sl = MockRepository.GenerateStub<IServiceLocator>();
                                       sl.Stub(x => x.GetInstance<TestService>("my_service")).Return(srv);
                                       return sl;
                                   });

            var service = c.Create();
            service.Start();

			service.State
                .ShouldEqual(ServiceState.Started);
		}
	}
}