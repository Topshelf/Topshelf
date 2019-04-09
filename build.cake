//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

// Load other scripts.
#load "./build/parameters.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

Setup<BuildParameters>(setupContext =>
{
    var buildParams = BuildParameters.GetParameters(setupContext);
    buildParams.Initialize(setupContext);
    return buildParams;
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does<BuildParameters>(data =>
{
    CleanDirectories($"./src/**/obj/{data.Configuration}");
    CleanDirectories($"./src/**/bin/{data.Configuration}");
    CleanDirectory(data.Paths.Directories.Artifacts);
});

Task("CleanAll")
    .Does<BuildParameters>(data =>
{
    CleanDirectories($"./src/**/obj");
    CleanDirectories($"./src/**/bin");
    CleanDirectory(data.Paths.Directories.Artifacts);
});

Task("Restore-NuGet")
    .Does<BuildParameters>(data =>
{
    DotNetCoreRestore(data.Paths.Directories.Solution.FullPath);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet")
    .Does<BuildParameters>(data =>
{
    var settings = new DotNetCoreBuildSettings{
        NoRestore = true,
        Configuration = data.Configuration,
        MSBuildSettings = new DotNetCoreMSBuildSettings().WithProperty("Version", data.Version.Version)
    };

    DotNetCoreBuild(data.Paths.Directories.Solution.FullPath, settings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does<BuildParameters>(data =>
{
    var settings = new DotNetCoreTestSettings
    {
        NoBuild = true,
        Configuration = data.Configuration,
        DiagnosticOutput = true
    };

    if(data.IsRunningOnAppVeyor) settings.ArgumentCustomization = args => args.Append($"--test-adapter-path:.").Append("--logger:Appveyor");

    DotNetCoreTest(data.Paths.Directories.Solution.FullPath, settings);
});

Task("Pack")
    .IsDependentOn("Build")
    .WithCriteria<BuildParameters>((context,data) => data.ShouldPublish)
    .Does<BuildParameters>(data =>
{
    var settings = new DotNetCorePackSettings{
        NoBuild = true,
        OutputDirectory = data.Paths.Directories.Artifacts,
        Configuration = data.Configuration,
        MSBuildSettings = new DotNetCoreMSBuildSettings().WithProperty("Version", data.Version.Version)
    };
    DotNetCorePack(data.Paths.Directories.Solution.FullPath, settings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
