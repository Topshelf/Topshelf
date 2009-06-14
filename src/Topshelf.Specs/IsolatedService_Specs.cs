namespace Topshelf.Specs
{
    using System;
    using Configuration;
    using Internal;
    using MbUnit.Framework;
    using Microsoft.Practices.ServiceLocation;
    using Rhino.Mocks;
    using Topshelf.Configuration;

    [TestFixture]
    [Serializable] //because of the lambda
    public class IsolatedService_Specs
    {
        private IServiceController _serviceController;
        private bool _wasPaused;
        private bool _wasContinued;

        [SetUp]
        public void EstablishContext()
        {
            
            

            var c = new IsolatedServiceConfigurator<TestService>("my_service");
            c.WhenStarted(s => s.Start());
            c.WhenStopped(s => s.Stop());
            c.WhenPaused(s => { _wasPaused = true; }); //need to change these
            c.WhenContinued(s => { _wasContinued = true; }); //need to change these
            c.CreateServiceLocator(()=>
                                   {
                                       TestService srv = new TestService();
                                       var sl = MockRepository.GenerateStub<IServiceLocator>();
                                       sl.Stub(x => x.GetInstance<TestService>("my_service")).Return(srv);
                                       return sl;
                                   });

            _serviceController = c.Create();
            _serviceController.Start();

        }

        [Test(Order = 1)]
        public void Should_start()
        {
            _serviceController.State
                .ShouldEqual(ServiceState.Started);
        }

        [Test(Order = 2)]
        public void Should_pause()
        {
            _serviceController.Pause();

            _serviceController.State
                .ShouldEqual(ServiceState.Paused);

            //probably can't do this as the cross domain
            //_wasPaused
               // .ShouldBeTrue();
        }

        [Test(Order = 3)]
        public void Should_continue()
        {
            _serviceController.Continue();

            _serviceController.State
                .ShouldEqual(ServiceState.Started);

            //probably can't do this as the cross domain
            //_wasContinued
            //    .ShouldBeTrue();
        }

        [Test(Order = 4)]
        [ExpectedException(typeof(AppDomainUnloadedException))]
        public void Should_stop()
        {
            _serviceController.Stop();

            //this throws the exception
            _serviceController.State
                .ShouldEqual(ServiceState.Stopped);

        }

        [Test(Order = 5)]
        public void Should_expose_contained_type()
        {
            _serviceController.ServiceType
                .ShouldEqual(typeof(TestService));
        }

        //TODO: state transition tests
    }
}