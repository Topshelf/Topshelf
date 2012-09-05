Topshelf - An easy service hosting framework for building Windows services using .NET
=======

![TopShelf](http://www.phatboyg.com/top-shelf-3-small.png "Topshelf")

# LICENSE
Apache 2.0 - see LICENSE

# INFO

## Getting started with TopShelf

Get started in four simple steps!

<dl>
	<dt>Step 1 (get the bits):</dt>
	<dd>
  <p>The easiest way to get Topshelf in your project is to use NuGet.</p>
  <p>If you aren't using NuGet, you can always <a href="https://github.com/Topshelf/Topshelf/downloads">download the code</a>.</p>
	</dd>
</dl>

### Mailing List

[Topshelf Discuss](http://groups.google.com/group/topshelf-discuss)

### Contributing

Make a pull request towards the `develop` branch.

### Source

 1. Clone the source down to your machine.

   ```
   git clone git://github.com/Topshelf/Topshelf.git
   git submodule init --update
   ```

 1. Install the build environment – a tutorial is [here][gems]
 1. `gem install bundler`
 1. `bundle`
 1. **Important:** Run `rake global_version` in order to generate the SolutionVersion.cs file which is otherwise missing.
	* You must have git on the path in order to do this. (Right click on `Computer` > `Advanced System Settings`, `Advanced` (tab) > `Environment Variables...` > Append the git executable's directory at the end of the PATH environment variable.
 1. Edit with Visual Studio 2010 or alternatively edit and run `rake`. `rake help` displays all possible tasks that you can run. The `package` task, is what the build server does.
 1. The default is .Net 4.0. At the moment, editing the solution file for .Net 3.5 requires the "fix" below.

[gems]: http://guides.rubyonrails.org/command_line.html  "How to use ruby/gems"

#### Editing in Visual Studio

 1. Run `rake global_version` in the root folder.
 2. Set Visual Studio Tools -> Options -> Text Editor -> All Languages -> Tabs to use "Tab Size" = 4, "Indent Size" = 4, and "Insert Spaces"
 3. Double-click/open the .sln file.

 You can alternatively edit and build for .Net 3.5 instead of .Net 4.0 with this step:

 * Edit all `TopShelf/src/**/*.csproj` files at the top of the files, where it says:
    `<TargetFrameworkVersion Condition=" '$(TargetFrameworkVersion)' == '' ">v4.0</TargetFrameworkVersion>`

   Change `v4.0` to `v3.5`. The build script itself won't be affected by this change, but it'll help Visual Studio know what version we're building for.
  3. Edit Topshelf.Host/app.config and uncomment the supportedRuntime- and runtime-elements.

#### Editing the rake script

 * Getting an overview: `rake help`
 * Getting descriptions of the tasks: `rake -P`
 * Read some articles; currently we need help with environments configuration and reducing the noise in tasks by making the files FileTask-s themselves. Some of this stuff is discussed [here][fowler-rake].

In general you should define your tasks to have the least number of dependencies to function. Paths should be placed in the props dictionary at the start of the rake file.

[fowler-rake]: http://martinfowler.com/articles/rake.html  "An article about Rake for building"

# REQUIREMENTS

To run the build, rake, .NET 3.5, and .NET 4.0 are required. To open the solution, you must have Visual Studio 2010 Service Pack 1.

# CREDITS
Logo Design by [The Agile Badger](http://www.theagilebadger.com)
Copyright 2007-2011 Travis Smith, Chris Patterson, Dru Sellers, Henrik Feldt et al. All rights reserved

