Topshelf - An easy service hosting framework for building Windows services using .NET
=======

Topshelf is a framework for hosting services written using the .NET framework. The creation of services is simplified, allowing developers to create a simple console application that can be installed as a service using Topshelf. The reason for this is simple: It is far easier to debug a console application than a service. And once the application is tested and ready for production, Topshelf makes it easy to install the application as a service.

### Develop build
[![Build status](https://ci.appveyor.com/api/projects/status/cjlqe1lg0733c936/branch/develop?svg=true)](https://ci.appveyor.com/project/phatboyg/topshelf)

# LICENSE
Apache 2.0 - see LICENSE

# INFO

## Getting started with Topshelf

Get started in four simple steps!

<dl>
	<dt>Step 1 (get the bits):</dt>
	<dd>
  <p>The easiest way to get Topshelf in your project is to use NuGet.</p>
	</dd>
</dl>

### Mailing List

[Topshelf Discuss](http://groups.google.com/group/topshelf-discuss)


### Contributing

1. Clone
1. Branch
1. Make changes
1. Push
1. Make a pull request

### Source

1. Clone the source down to your machine.
   `git clone git://github.com/Topshelf/Topshelf.git`
1. **Important:** Run `build.bat` in order to generate the SolutionVersion.cs file which is otherwise missing.
	* You must have git on the path in order to do this. (Right click on `Computer` > `Advanced System Settings`, `Advanced` (tab) > `Environment Variables...` > Append the git executable's directory at the end of the PATH environment variable.
1. Edit with Visual Studio 2015 or alternatively edit and run `build.bat`.
1. Topshelf uses the .NET Framework v4.5.2.

#### Editing in Visual Studio

1. Run `build.bat` in the root folder.
2. Set Visual Studio Tools -> Options -> Text Editor -> All Languages -> Tabs to use "Tab Size" = 4, "Indent Size" = 4, and "Insert Spaces"
3. Double-click/open the .sln file.

### Deploying TopShelf with Azure DevOps

The [Windows Service Manager](https://marketplace.visualstudio.com/items?itemName=MDSolutions.WindowsServiceManagerWindowsServiceManager) Azure DevOps extension supports TopShelf deployments to a group of target machines or a deployment group target.

# REQUIREMENTS

To run the build, a Visual Studio 2015 compatible environment should be setup.

# CREDITS
Logo Design by [The Agile Badger](http://www.theagilebadger.com)

Copyright 2007-2016 Travis Smith, Chris Patterson, Dru Sellers, Henrik Feldt et al. All rights reserved

