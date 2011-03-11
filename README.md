Topshelf - An easy service hosting framework for building Windows services using .NET
=======

# LICENSE
Apache 2.0 - see LICENSE

# IMPORTANT
NOTE: If you are looking at the source - please run build.bat before opening the solution. It creates the SolutionVersion.cs file that is necessary for a successful build.

## Overview
# INFO
Topshelf is an awesome service host for windows services.

## Getting started with Mass Topshelf

### Mailing List

[Topshelf Discuss](http://groups.google.com/group/topshelf-discuss)

### Source

 1. Clone the source down to your machine. 
   `git clone git://github.com/Topshelf/Topshelf.git`
2. Run `build.bat`. NOTE: You must have git on the path (open a regular command line and type git).
3. If you want to edit the project in 4.0 mode you will have to edit all .csproj files at the top of the files, where it says:
    <TargetFrameworkVersion Condition=" '$(TargetFrameworkVersion)' == '' ">v3.5</TargetFrameworkVersion> 
   or alternatively
    <TargetFrameworkVersion Condition=" '$(TargetFrameworkVersion)' == '' ">v4.0</TargetFrameworkVersion>
   depending on whose copy you've cloned. Edit it to match your preferred mode of coding in Visual Studio.
   
   Furthermore, you can edit rakefile.rb and set BUILD_CONFIG_KEY appropriately, to either NET40 or NET35.
   Finally, changing it while using R# might cause cache corruption, and so you'd have to delete the _ReSharper
   folder in your project in order to get solution-wide analysis going again.

### Contributing 

1. `git config --global core.autoclrf false`
2. Shared ReSharper settings are under src/Topshelf.resharper.xml for formatting and style. ReSharper should pick up on this automatically when you launch Visual Studio.
3. Make a pull request

    
# REQUIREMENTS
* .NET Framework 3.5 
* It isn't yet capable of hosting .Net Framework 4.0 Applications.

# CREDITS
Logo Design by [The Agile Badger](http://www.theagilebadger.com)  
UppercuT - Automated Builds in moments, not days! [Project UppercuT](http://projectuppercut.org)
