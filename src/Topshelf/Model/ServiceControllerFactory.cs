// Copyright 2007-2010 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Model
{
	using System;
	using Exceptions;
	using log4net;
	using Messages;
	using Stact;
	using Stact.Workflow;
	using Stact.Workflow.Configuration;


	public class ServiceControllerFactory
	{
		readonly StateMachineWorkflow<IServiceWorkflow, IServiceController> _workflow;
		readonly StateMachineWorkflowBinder<IServiceWorkflow, IServiceController> _workflowBinder;

		public ServiceControllerFactory()
		{
			_workflow = StateMachineWorkflow.New<IServiceWorkflow, IServiceController>(ConfigureWorkflow);

			_workflowBinder = new StateMachineWorkflowBinder<IServiceWorkflow, IServiceController>(_workflow);
		}

		public WorkflowDefinition<IServiceWorkflow> Workflow
		{
			get { return _workflow; }
		}

		public ActorFactory<TController> CreateFactory<TController>(Func<Inbox, TController> createInstance)
			where TController : class, IServiceController
		{
			ActorFactory<TController> factory = ActorFactory.Create<TController>(x =>
				{
					x.ConstructedBy(inbox =>
						{
							TController instance = createInstance(inbox);

							_workflowBinder.Bind(inbox, instance);

							return instance;
						});
				});

			return factory;
		}

		static void ConfigureWorkflow(StateMachineConfigurator<IServiceWorkflow, IServiceController> x)
		{
			x.AccessCurrentState(s => s.CurrentState);

			x.Initially()
				.When(e => e.Create)
				.TransitionTo(s => s.Creating)
				.Then(i => i.Create)
				.InCaseOf()
				.Exception<BuildServiceException>()
				.TransitionTo(s => s.Faulted);

			x.During(s => s.Creating)
				.When(e => e.OnCreated)
				.TransitionTo(s => s.Created)
				.TransitionTo(s => s.Starting)
				.Then(i => i.Start)
				.InCaseOf()
				.Exception<Exception>()
				.TransitionTo(s => s.Faulted);

			x.During(s => s.Creating)
				.AcceptFault();

			x.During(s => s.Creating)
				.When(e => e.Stop)
				.TransitionTo(s => s.StopRequested);

			x.During(s => s.Created)
				.When(e => e.Start)
				.TransitionTo(s => s.Starting)
				.Then(i => i.Start)
				.InCaseOf()
				.Exception<Exception>()
				.TransitionTo(s => s.Faulted);

			x.During(s => s.Created)
				.When(e => e.OnRunning)
				.TransitionTo(s => s.Running)
				.AcceptFault();

			x.During(s => s.Starting)
				.When(e => e.OnRunning)
				.TransitionTo(s => s.Running)
				.When(e => e.Stop)
				.TransitionTo(s => s.StopRequested)
				.AcceptFault();

			x.During(s => s.Running)
				.AcceptStop()
				.AcceptPause()
				.AcceptRestart()
				.AcceptFault();


			x.During(s => s.StopRequested)
				.When(e => e.OnRunning)
				.Then(i => i.Stop)
				.TransitionTo(s => s.Stopping)
				.When(e => e.OnFaulted)
				.TransitionTo(s => s.Faulted)
				.Then(i => i.Stop);

			x.During(s => s.StopRequested)
				.When(e => e.OnCreated)
				.TransitionTo(s => s.Created)
				.Then(i => i.Stop)
				.TransitionTo(s => s.Stopping)
				.InCaseOf()
				.Exception<Exception>()
				.TransitionTo(s => s.Faulted);

			x.During(s => s.Stopping)
				.When(e => e.OnStopped)
				.TransitionTo(s => s.Stopped)
				.AcceptFault();

			x.During(s => s.Pausing)
				.When(e => e.OnPaused)
				.TransitionTo(s => s.Paused)
				.AcceptFault();

			x.During(s => s.Paused)
				.When(e => e.Continue)
				.TransitionTo(s => s.Continuing)
				.Then(i => i.Continue)
				.AcceptFault();

			x.During(s => s.Continuing)
				.When(e => e.OnRunning)
				.TransitionTo(s => s.Running)
				.AcceptFault();

			x.During(s => s.Stopped)
				.When(e => e.Unload)
				.TransitionTo(s => s.Unloading)
				.Then(i => i.Unload);

			x.During(s => s.Unloading)
				.When(e => e.OnUnloaded)
				.TransitionTo(s => s.Completed);

			x.During(s => s.Faulted)
				.When(e => e.Restart)
				.Then(i => i.Create)
				.TransitionTo(s => s.CreatingToRestart);

			x.During(s => s.StoppingToRestart)
				.When(e => e.OnStopped)
				.TransitionTo(s => s.UnloadingToRestart)
				.Then(i => i.Unload)
				.AcceptFault();

			x.During(s => s.UnloadingToRestart)
				.When(e => e.OnUnloaded)
				.TransitionTo(s => s.CreatingToRestart)
				.Then(i => i.Create)
				.AcceptFault();

			x.During(s => s.CreatingToRestart)
				.AcceptFault();

			x.During(s => s.CreatingToRestart)
				.When(e => e.OnCreated)
				.TransitionTo(s => s.Created)
				.TransitionTo(s => s.Restarting)
				.Then(i => i.Start())
				.InCaseOf()
				.Exception<Exception>()
				.Then((i, m) => { }) // m.Respond(new ServiceFault(i.Name, null /* TODO get ex */)))
				.TransitionTo(s => s.Faulted);

			x.During(s => s.Restarting)
				.When(e => e.OnRunning)
				.TransitionTo(s => s.Running)
				.AcceptFault();

			x.During(s => s.Faulted)
				.When(e => e.Stop)
				.TransitionTo(s => s.Completed);
		}
	}


	/// <summary>
	/// These are to make the DSL read easier for exception handling, showing how keywords
	/// can be added to the DSL without a lot of effort! :)
	/// </summary>
	public static class ServiceWorkflowExtensions
	{
		static readonly ILog _log = LogManager.GetLogger("Topshelf.Model.ServiceWorkflow");

		public static ActivityConfigurator<IServiceWorkflow, IServiceController, StopService> AcceptStop(
			this StateConfigurator<IServiceWorkflow, IServiceController> configurator)
		{
			return configurator
				.When(e => e.Stop)
				.TransitionTo(s => s.Stopping)
				.Then(i => i.Stop);
		}

		public static ActivityConfigurator<IServiceWorkflow, IServiceController, PauseService> AcceptPause(
			this StateConfigurator<IServiceWorkflow, IServiceController> configurator)
		{
			return configurator
				.When(e => e.Pause)
				.TransitionTo(s => s.Pausing)
				.Then(i => i.Pause);
		}

		public static ActivityConfigurator<IServiceWorkflow, IServiceController, ServiceFault> AcceptFault(
			this StateConfigurator<IServiceWorkflow, IServiceController> configurator)
		{
			return configurator
				.When(e => e.OnFaulted)
				.LogFault()
				.TransitionTo(s => s.Faulted);
		}

		public static ActivityConfigurator<IServiceWorkflow, IServiceController, RestartService> AcceptRestart(
			this StateConfigurator<IServiceWorkflow, IServiceController> configurator)
		{
			return configurator
				.When(e => e.Restart)
				.TransitionTo(s => s.StoppingToRestart)
				.Then(i => i.Stop);
		}

		public static ActivityConfigurator<IServiceWorkflow, IServiceController, ServiceFault> LogFault(
			this ActivityConfigurator<IServiceWorkflow, IServiceController, ServiceFault> configurator)
		{
			return configurator
				.Then((i, m) => { _log.ErrorFormat("[{0}] Fault: {1}", i.Name, m.ToLogString()); });
		}
	}
}