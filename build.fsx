#r @"src/packages/FAKE/tools/FakeLib.dll"
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.Git.Information
open Fake.SemVerHelper

let buildOutputPath = "./build_output"
let buildArtifactPath = "./build_artifacts"
let nugetWorkingPath = FullName "./build_temp"
let packagesPath = FullName "./src/packages"
let keyFile = FullName "./Topshelf.snk"

let assemblyVersion = "4.0.0.0"
let baseVersion = "4.0.3"

let semVersion : SemVerInfo = parse baseVersion

let Version = semVersion.ToString()

let branch = (fun _ ->
  (environVarOrDefault "APPVEYOR_REPO_BRANCH" (getBranchName "."))
)

let FileVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (Version + "." + "0"))

let informationalVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else " (" + branchName + "/" + (getCurrentSHA1 ".").[0..7] + ")"
  (FileVersion + label)
)

let nugetVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else "-" + (if branchName="mt3" then "beta" else branchName)
  (Version + label)
)

let InfoVersion = informationalVersion()
let NuGetVersion = nugetVersion()


printfn "Using version: %s" Version

Target "Clean" (fun _ ->
  ensureDirectory buildOutputPath
  ensureDirectory buildArtifactPath
  ensureDirectory nugetWorkingPath

  CleanDir buildOutputPath
  CleanDir buildArtifactPath
  CleanDir nugetWorkingPath
)

Target "RestorePackages" (fun _ -> 
     "./src/Topshelf.sln"
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = packagesPath
             Retries = 4 })
)

Target "Build" (fun _ ->

  CreateCSharpAssemblyInfo @".\src\SolutionVersion.cs"
    [ Attribute.Title "Topshelf"
      Attribute.Description "Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service."
      Attribute.Product "Topshelf"
      Attribute.Version assemblyVersion
      Attribute.FileVersion FileVersion
      Attribute.InformationalVersion InfoVersion
      Attribute.Copyright "Copyright 2012 Chris Patterson, Dru Sellers, Travis Smith, All rights reserved."
    ]

  let buildMode = getBuildParamOrDefault "buildMode" "Release"
  let setParams defaults = { 
    defaults with
        Verbosity = Some(Quiet)
        Targets = ["Clean"; "Build"]
        Properties =
            [
                "Optimize", "True"
                "DebugSymbols", "True"
                "Configuration", buildMode
                "TargetFrameworkVersion", "v4.5.2"
                "Platform", "Any CPU"
            ]
  }

  build setParams @".\src\Topshelf.sln"
      |> DoNothing

  let unsignedSetParams defaults = { 
    defaults with
        Verbosity = Some(Quiet)
        Targets = ["Build"]
        Properties =
            [
                "Optimize", "True"
                "DebugSymbols", "True"
                "Configuration", "ReleaseUnsigned"
                "TargetFrameworkVersion", "v4.5.2"
                "Platform", "Any CPU"
            ]
  }

  build unsignedSetParams @".\src\Topshelf.sln"
      |> DoNothing
)

type packageInfo = {
    Project: string
    PackageFile: string
    Summary: string
    Files: list<string*string option*string option>
}

Target "Package" (fun _ ->

  let nugs = [| { Project = "Topshelf"
                  Summary = "Topshelf Service Library"
                  PackageFile = @".\src\Topshelf\packages.config"
                  Files = [ (@"..\src\Topshelf\bin\Release\Topshelf.*", Some @"lib\net452", None);
                            (@"..\src\Topshelf\**\*.cs", Some "src", None) ] }
                { Project = "Topshelf.Log4Net"
                  Summary = "Topshelf Log4Net Integration."
                  PackageFile = @".\src\Topshelf.Log4Net\packages.config"
                  Files = [ (@"..\src\Topshelf.Log4Net\bin\Release\Topshelf.Log4Net.*", Some @"lib\net452", None);
                            (@"..\src\Topshelf.Log4Net\**\*.cs", Some @"src", None) ] } 
                { Project = "Topshelf.NLog"
                  Summary = "Topshelf NLog Integration."
                  PackageFile = @".\src\Topshelf.NLog\packages.config"
                  Files = [ (@"..\src\Topshelf.NLog\bin\Release\Topshelf.NLog.*", Some @"lib\net452", None);
                            (@"..\src\Topshelf.NLog\**\*.cs", Some @"src", None) ] } 
                { Project = "Topshelf.Elmah"
                  Summary = "Topshelf Elmah Integration."
                  PackageFile = @".\src\Topshelf.Elmah\packages.config"
                  Files = [ (@"..\src\Topshelf.Elmah\bin\Release\Topshelf.Elmah.*", Some @"lib\net452", None);
                            (@"..\src\Topshelf.Elmah\**\*.cs", Some @"src", None) ] } 
                { Project = "Topshelf.Serilog"
                  Summary = "Topshelf Serilog Integration."
                  PackageFile = @".\src\Topshelf.Serilog\packages.config"
                  Files = [ (@"..\src\Topshelf.Serilog\bin\Release\Topshelf.Serilog.*", Some @"lib\net452", None);
                            (@"..\src\Topshelf.Serilog\**\*.cs", Some @"src", None) ] } 
             |]

  nugs
    |> Array.iter (fun nug ->

      let getDeps daNug : NugetDependencies =
        if daNug.Project = "Topshelf" then (getDependencies daNug.PackageFile)
        else ("Topshelf", NuGetVersion) :: (getDependencies daNug.PackageFile)

      let setParams defaults = {
        defaults with 
          Authors = ["Chris Patterson"; "Dru Sellers"; "Travis Smith"; "Brian Wilson"; "Mogens Heller Grabe"]  
          Description = "Topshelf is an open source project for hosting services without friction. By referencing Topshelf, your console application *becomes* a service installer with a comprehensive set of command-line options for installing, configuring, and running your application as a service."
          OutputPath = buildArtifactPath
          Project = nug.Project
          Dependencies = (getDeps nug)
          Summary = nug.Summary
          SymbolPackage = NugetSymbolPackage.Nuspec
          Version = NuGetVersion
          WorkingDir = nugetWorkingPath
          Files = nug.Files
      } 

      NuGet setParams (FullName "./template.nuspec")
    )
)

Target "Default" (fun _ ->
  trace "Build starting..."
)

"Clean"
  ==> "RestorePackages"
  ==> "Build"
  ==> "Package"
  ==> "Default"

RunTargetOrDefault "Default"