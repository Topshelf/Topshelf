namespace Topshelf.Specs
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using Internal;
    using Microsoft.Practices.ServiceLocation;
    using NUnit.Framework;
    using Rhino.Mocks;

    [TestFixture]
    public class ServiceCoordinator_Singles_Specs
    {
        private TestService _service;
        private TestService2 _service2;
        private ServiceCoordinator _serviceCoordinator;

        [SetUp]
        public void EstablishContext()
        {
            _service = new TestService();
            _service2 = new TestService2();
            IServiceLocator sl = MockRepository.GenerateMock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => sl);

            sl.Stub(x => x.GetInstance<TestService>("test")).Return(_service).Repeat.Any();
            sl.Stub(x => x.GetInstance<TestService2>("test2")).Return(_service2).Repeat.Any();

            _serviceCoordinator = new ServiceCoordinator(x => { }, x => { }, x => { });
            IList<Func<IServiceController>> services = new List<Func<IServiceController>>
                                                           {
                                                               ()=>
                                                               new ServiceController<TestService>
                                                                   {
                                                                       CreateServiceLocator = () => sl,
                                                                       Name = "test",
                                                                       StartAction = x => x.Start(),
                                                                       StopAction = x => x.Stop(),
                                                                       ContinueAction = x => x.Continue(),
                                                                       PauseAction = x => x.Pause()
                                                                   },
                                                               ()=>
                                                               new ServiceController<TestService2>
                                                                   {
                                                                       CreateServiceLocator = () => sl,
                                                                       Name = "test2",
                                                                       StartAction = x => x.Start(),
                                                                       StopAction = x => x.Stop(),
                                                                       ContinueAction = x => x.Continue(),
                                                                       PauseAction = x => x.Pause()
                                                                   }
                                                           };

            _serviceCoordinator.RegisterServices(services);
            _serviceCoordinator.Start();
        }

        [Test]
        public void Stop_individual_service()
        {
            _serviceCoordinator.StopService("test");

            _service.Stopped
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }

        [Test]
        public void Start_individual_service()
        {
            _serviceCoordinator.Pause(); //should pause them all?
            _serviceCoordinator.ContinueService("test");

            _service.Started
                .ShouldBeTrue();
            _service2.Paused
                .ShouldBeTrue();
        }

        [Test]
        public void Pause_individual_service()
        {
            _serviceCoordinator.PauseService("test");

            _service.Paused
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }

        [Test]
        public void D_Continue_individual_service()
        {
            _serviceCoordinator.PauseService("test");
            _serviceCoordinator.ContinueService("test");

            _service.Started
                .ShouldBeTrue();
            _service.HasBeenContinued
                .ShouldBeTrue();
            _service2.Started
                .ShouldBeTrue();
        }
    }
}