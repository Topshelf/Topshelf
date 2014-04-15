// copyright Henrik Feldt 2014

namespace Topshelf

[<AutoOpen>]
module FSharpApi =

  open System
  open Topshelf
  open Topshelf.HostConfigurators
  open Topshelf.Runtime

  let with_topshelf f =
    HostFactory.Run(new Action<_>(f)) |> int

  let add_command_line_definition (conf : HostConfigurator) str action =
    conf.AddCommandLineDefinition(str, new Action<_>(action))

  let add_command_line_switch (conf : HostConfigurator) str action =
    conf.AddCommandLineSwitch(str, new Action<_>(action))

  let add_dependency (conf : HostConfigurator) dep_name =
    conf.AddDependency dep_name |> ignore

  let before_install (conf : HostConfigurator) f =
    conf.BeforeInstall(new Action<InstallHostSettings>(f)) |> ignore

  let after_install (conf : HostConfigurator) f =
    conf.AfterInstall(new Action<InstallHostSettings>(f)) |> ignore

  let apply_command_line (conf : HostConfigurator) str =
    conf.ApplyCommandLine str

  let before_uninstall (conf : HostConfigurator) f =
    conf.BeforeUninstall(new Action(f)) |> ignore

  let after_uninstall (conf : HostConfigurator) f =
    conf.AfterUninstall(new Action(f)) |> ignore

  let depends_on (conf : HostConfigurator) name =
    conf.DependsOn name |> ignore

  let depends_on_eventlog (conf : HostConfigurator) =
    conf.DependsOnEventLog() |> ignore

  let depends_on_iis (conf : HostConfigurator) =
    conf.DependsOnIis() |> ignore

  let depends_on_mssql (conf : HostConfigurator) = 
    conf.DependsOnMsSql() |> ignore

  let depends_on_rabbitmq (conf : HostConfigurator) =
    "RabbitMQ" |> depends_on conf

  let depends_on_msmq (conf : HostConfigurator) =
    conf.DependsOnMsmq() |> ignore

  let disabled (conf : HostConfigurator) =
    conf.Disabled() |> ignore

  let enable_pause_and_continue (conf : HostConfigurator) =
    conf.EnablePauseAndContinue()

  let enable_service_recovery (conf : HostConfigurator) f =
    conf.EnableServiceRecovery(new Action<_>(f)) |> ignore

  let enable_shutdown (conf : HostConfigurator) =
    conf.EnableShutdown()

  let load_help_text_prefix (conf : HostConfigurator) asm str =
    conf.LoadHelpTextPrefix( asm, str ) |> ignore

  let run_as (conf : HostConfigurator) usr pwd =
    conf.RunAs(usr, pwd) |> ignore

  let run_as_network_service (conf : HostConfigurator) =
    conf.RunAsNetworkService() |> ignore

  let run_as_local_system (conf : HostConfigurator) =
    conf.RunAsLocalSystem() |> ignore

  let run_as_local_service (conf : HostConfigurator) =
    conf.RunAsLocalService() |> ignore

  let run_as_prompt (conf : HostConfigurator) =
    conf.RunAsPrompt() |> ignore

  let help_text_prefix (conf : HostConfigurator) str =
    conf.SetHelpTextPrefix str |> ignore

  let start_auto (conf : HostConfigurator) =
    conf.StartAutomatically() |> ignore

  let start_auto_delayed (conf : HostConfigurator) =
    conf.StartAutomaticallyDelayed() |> ignore

  let start_manually (conf : HostConfigurator) =
    conf.StartManually() |> ignore

  let use_env_builder (conf : HostConfigurator) f =
    conf.UseEnvironmentBuilder(new EnvironmentBuilderFactory(f))

  let use_host_builder (conf : HostConfigurator) f =
    conf.UseHostBuilder(new HostBuilderFactory(f))

  let use_service_builder (conf : HostConfigurator) f =
    conf.UseServiceBuilder(new ServiceBuilderFactory(f))

  let use_test_host (conf : HostConfigurator) =
    conf.UseTestHost() |> ignore

  /// create a service given a service control factory
  let service (conf : HostConfigurator) (fac : (unit -> 'a)) =
    let service' = conf.Service : Func<_> -> HostConfigurator
    service' (new Func<_>(fac)) |> ignore

  /// create a service control from a start and a stop function
  let service_control (start : HostControl -> bool) (stop : HostControl -> bool) =
    { new ServiceControl with
        member x.Start hc =
          start hc
        member x.Stop hc =
          stop hc }

  /// A module for handling the naming of the service. A part of the fluent configuration
  /// API.
  [<AutoOpen>]
  module Naming =
    let service_name (conf : HostConfigurator) str =
      conf.SetServiceName str

    let instance_name (conf : HostConfigurator) str =
      conf.SetInstanceName str

    let display_name (conf : HostConfigurator) str =
      conf.SetDisplayName str

    let description (conf : HostConfigurator) str =
      conf.SetDescription str

    let naming_from_asm (conf : HostConfigurator) asm =
      HostConfiguratorExtensions.UseAssemblyInfoForServiceInfo(conf, asm)

    let naming_from_this_asm (conf : HostConfigurator) =
      HostConfiguratorExtensions.UseAssemblyInfoForServiceInfo conf

  [<AutoOpen>]
  module Recovery =
    let with_recovery (conf : HostConfigurator) f =
      ServiceRecoveryConfiguratorExtensions.EnableServiceRecovery(conf,
        new Action<_>(f))

    let restart (span : TimeSpan) (c : ServiceRecoveryConfigurator) =
      c.RestartService(int span.TotalMinutes) |> ignore

    let restart_computer (span : TimeSpan) message (c : ServiceRecoveryConfigurator) =
      c.RestartComputer(int span.TotalMinutes, message) |> ignore

    let run_program (span : TimeSpan) cmd (c : ServiceRecoveryConfigurator) =
      c.RunProgram(int span.TotalMinutes, cmd) |> ignore

    let set_reset_period (days : TimeSpan) (c : ServiceRecoveryConfigurator) =
      c.SetResetPeriod(int days.TotalDays) |> ignore

    let on_crash_only (c : ServiceRecoveryConfigurator) =
      c.OnCrashOnly()

  /// A module for making constructing times nicer with F#, not a part of the
  /// fluent configuration API.
  module Time =
    let directly = TimeSpan.FromMilliseconds 0.
    let ms (i : int) = TimeSpan.FromMilliseconds(float i)
    let s (i : int) = TimeSpan.FromSeconds (float i)
    let min (i : int) = TimeSpan.FromMinutes (float i)
    let h (i : int) = TimeSpan.FromHours (float i)
    let d (i : int) = TimeSpan.FromDays (float i)

  /// A module that wraps the calls to HostControl, not a part of the fluent
  /// configuration API.
  module HostControl =
    open System

    let request_more_time (hc : HostControl) time =
      hc.RequestAdditionalTime time

    let restart (hc : HostControl) =
      hc.Restart()

    let stop (hc : HostControl) =
      hc.Stop()
