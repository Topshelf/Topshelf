Topshelf - An easy service hosting framework for building Windows services using .NET
=======

# LICENSE
Apache 2.0 - see LICENSE

# INFO
Topshelf is an awesome service host for windows services. Choose between two modes of operation by either having your services *Shelved* or *Installed*. A quick 4-step guide to get started can be found below.

## Getting started with TopShelf

Get started in four simple steps!

<dl>
	<dt>Step 1 (get the bits):</dt>
	<dd>
		<p><a href="https://github.com/Topshelf/Topshelf/downloads">Download the code</a></p>
	</dd>
	
	<dt>Step 2 (pick your poison):</dt>
	<dd>
		<p>Choose whether you want your service <a href="http://topshelf-project.com/documentation/shelving/">Shelved</a> or <a href="http://topshelf-project.com/documentation/getting-started/" title="How to link your services to enable installing out-of-the-box">Installed</a>. My personal preference is the Shelving-feature.</p>
	</dd>
	
	<dt>Step 3 (use examples):</dt>
	<dd>
		<h5>For shelving</h5>
		<p>Extract the downloaded files and execute <code>Topshelf.Host.exe run</code> in the folder with the binaries. Watch the log folder for output.</p>
		
		<h5>For installing</h5>
		<p>Go to the <code>Examples/clock</code> folder and run <code>Clock.exe install</code>
	</dd>
	
	<dt>Step 4 (use it yourself):</dt>
	<dd>
		<p>Add a reference to the Topshelf.dll-dll as can be seen in the examples.</p>
	</dd>
</dl>

### Mailing List

[Topshelf Discuss](http://groups.google.com/group/topshelf-discuss)

   
### Contributing 

1. `git config --global core.autoclrf false`
2. Shared ReSharper settings are under src/Topshelf.resharper.xml for formatting and style. ReSharper should pick up on this automatically when you launch Visual Studio.
3. Make a pull request

### Source

 1. Clone the source down to your machine. 
   `git clone git://github.com/Topshelf/Topshelf.git`
 2. Download git, ruby and gems. Install – a tutorial is [here][gems]
 3. Install albacore. Run "gem install albacore"
 4. **Important:** Run `rake global_version` in order to generate the SolutionVersion.cs file which is otherwise missing. 
	* You must have git on the path in order to do this. (Right click on `Computer` > `Advanced System Settings`, `Advanced` (tab) > `Environment Variables...` > Append the git executable's directory at the end of the PATH environment variable.
 5. Edit with Visual Studio 2010 or alternatively edit and run `rake`. `rake help` displays all possible tasks that you can run. The `package` task, is what the build server does.
 6. The default is .Net 4.0. At the moment, editing the solution file for .Net 3.5 requires the "fix" below.
 7. In order to debug one of your services together with Topshelf:
	a) set Topshelf.Host as the startup project
	b) add "run" as parameters when Topshelf.Host starts (in the debug tab of properties)
	c) Add your service to the 'Services' folder.
	d) (open your own files and place debug points in them)
	
[gems]: http://guides.rubyonrails.org/command_line.html  "How to use ruby/gems"

#### Editing in Visual Studio

 1. Run `rake global_version` in the root folder.
 2. Double-click/open the .sln file.
 
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
 
### Contributing: what do we need help with?

Have a look at [the issues page](https://github.com/Topshelf/Topshelf/issues). Issues range from the advanced, e.g. improving the service-hosting system with service-per-process to the simple, e.g. improving the rake file or writing code summaries for the classes inside the .cs files.
  
## Some hints
 * Have a look at the readme-files along with the samples
 * Make sure that your Shelved services don't change anything inside the 'Services' folder.
 * Your service's .config-file needs to be named 'ServiceName.config', even if your dll is named 'ServiceName.dll' and App.config
   would be transformed into 'ServiceName.dll.config'. 'ServiceName.dll.config' won't be searched for when looking for a bootstrapper.
 * When you use your `Bootstrapper<T>`, you have the option of either calling `HowToBuildService`, or `ConstructUsing`. 
   HowToBuildService actually calls `ConstructUsing` internally and you need the third argument -- the communication channel. This is
   the signature of the delegate you pass to ConstructUsing:

    `public delegate TService DescriptionServiceFactory< TService >( ServiceDescription description,  string name,  IServiceChannel coordinatorChannel) : MulticastDelegate`

# REQUIREMENTS

<table>
	<thead>
		<tr>
			<th>Framework</th>
			<th>Build command</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>.NET Framework 3.5</td>
			<td><code>rake</code></td>
		</tr>
		<tr>
			<td>.Net Framework 4.0</td>
			<td><code>rake BUILD_CONFIG_KEY=NET35</code></td>
		</tr>
	</tbody>
</table>

# CREDITS
Logo Design by [The Agile Badger](http://www.theagilebadger.com)  
Copyright 2007-2011 Travis Smith, Chris Patterson, Dru Sellers, Henrik Feldt et al. All rights reserved