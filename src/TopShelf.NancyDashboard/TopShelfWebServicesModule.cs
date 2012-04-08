namespace Topshelf.NancyDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Threading;
    using Stact;
    using Stact.MessageHeaders;
    using Logging;
    using Messages;
    using Model;
    using Nancy;


    public class TopshelfWebServicesModule : NancyModule
    {
        readonly ILog _log = Logger.Get("Topshelf.WebControl.TopShelfControlModule");
        public dynamic Model = new ExpandoObject();

        public TopshelfWebServicesModule()
        {
            var serviceCoordinator = TinyIoC.TinyIoCContainer.Current.Resolve<IServiceChannel>();

            Get["/"] = x =>
                {
                    Model.Title = "Topshelf";
                    Model.Services = GetServices(serviceCoordinator);

                    return View["dashboard.html", Model];
                };

            Get["/services"] = x =>
                {
                    return Response.AsJson(GetServices(serviceCoordinator));
                };

            Get["/service/{name}/start"] = parms =>
            {
                string name = parms.name;

                serviceCoordinator.Send(new StartService(name));

                return HttpStatusCode.OK;
            };

            Get["/service/{name}/stop"] = parms =>
            {
                string name = parms.name;

                serviceCoordinator.Send(new StopService(name));

                return HttpStatusCode.OK;
            };

            Get["/service/{name}/unload"] = parms =>
            {
                string name = parms.name;

                serviceCoordinator.Send(new UnloadService(name));

                return HttpStatusCode.OK;
            };

            Get["/images/{file}"] = @params => Response.AsImage("." + Request.Path);
            
            Get["/styles/{file}"] = @params => Response.AsCss("." + Request.Path);
        }

        private IEnumerable<dynamic> GetServices(IServiceChannel serviceCoordinator)
        {
            var handle = new AutoResetEvent(false);

            IEnumerable<dynamic> report = null;

            AnonymousActor.New(inbox =>
            {
                serviceCoordinator.Send<Request<ServiceStatus>>(new RequestImpl<ServiceStatus>(inbox, new ServiceStatus()));

                inbox.Receive<Response<ServiceStatus>>(response =>
                    {
                        report = from service in response.Body.Services
                                 select new
                                     {
                                         Name = Uri.EscapeUriString(service.Name),
                                         service.ServiceType,
                                         service.CurrentState,
                                         Action = (service.CurrentState == "Running") ? "stop" : "start"
                                     };

                    handle.Set();

                }, TimeSpan.FromSeconds(30), () => handle.WaitOne());
            });

            handle.WaitOne();

            return report;
        }
    }
}