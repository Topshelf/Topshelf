// copyright Henrik Feldt 2014

#r "bin/Debug/Topshelf.dll"
#load "FSharpApi.fs"

open System.Reflection
[<assembly: AssemblyTitle("Sample Service")>]
()

open System

open Topshelf
open Time

[<EntryPoint>]
let main args =
  let info : string -> unit = fun s -> Console.WriteLine(sprintf "%s logger/sample-service: %s" (DateTime.UtcNow.ToString("o")) s)
  let sleep (time : TimeSpan) = System.Threading.Thread.Sleep(time)

  with_topshelf <| fun conf ->
    run_as_network_service conf

    naming_from_this_asm conf

    with_recovery conf (restart (s 20))

    service conf <| fun _ ->
      { new ServiceControl with
          member x.Start hc =
            info "sample service starting"

            (s 30) |> HostControl.request_more_time hc
            sleep (s 1)

            Threading.ThreadPool.QueueUserWorkItem(fun cb ->
              sleep (s 3)
              info "requesting stop"
              hc |> HostControl.stop) |> ignore

            info "sample service started"
            true

          member x.Stop hc =
            info "sample service stopped"
            true
      }
