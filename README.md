Topshelf - An easy service hosting framework for building Windows services using .NET
=======

# LICENSE
Apache 2.0 - see LICENSE

# IMPORTANT
NOTE: If you are looking at the source - please run build.bat before opening the solution. It creates the SolutionVersion.cs file that is necessary for a successful build.

## Overview
# INFO
Topshelf is an awesome service host for windows services.

## Getting started with TopShelf

We are currently working on more documentation in order to get started. In principle, download the zip file.

### Mailing List

[Topshelf Discuss](http://groups.google.com/group/topshelf-discuss)

### Source

 1. Clone the source down to your machine. 
   `git clone git://github.com/Topshelf/Topshelf.git`
 2. Run `build.bat`. NOTE: You must have git on the path (open a regular command line and type git).
 3. OR: Run `rake`. You can build for .Net 3.5 with 

#### Editing in Visual Studio

 1. Run `rake` in the root folder.
 2. Double-click/open the .sln file.
 
 You can alternatively edit and build for .Net 3.5 instead of .Net 4.0 with this step:
 
 * Edit all `TopShelf/src/**/*.csproj` files at the top of the files, where it says:
    `<TargetFrameworkVersion Condition=" '$(TargetFrameworkVersion)' == '' ">v4.0</TargetFrameworkVersion 
   
   Change `v4.0` to `v3.5`. The build script itself won't be affected by this change, but it'll help Visual Studio know what version we're building for.
   
### Contributing 

1. `git config --global core.autoclrf false`
2. Shared ReSharper settings are under src/Topshelf.resharper.xml for formatting and style. ReSharper should pick up on this automatically when you launch Visual Studio.
3. Make a pull request

    
# REQUIREMENTS
* .NET Framework 3.5
* Alternative build: .Net Framework 4.0

# CREDITS
Logo Design by [The Agile Badger](http://www.theagilebadger.com)  
UppercuT - Automated Builds in moments, not days! [Project UppercuT](http://projectuppercut.org)
