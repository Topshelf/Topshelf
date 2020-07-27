Installing Topshelf
===================

NuGet
'''''

The simplest way to install Topshelf into your solution/project is to use
NuGet.::

    nuget Install-Package Topshelf


Raw Binaries
''''''''''''

If you are a fan of getting the binaries you can get released builds from ``http://github.com/topshelf/Topshelf/downloads`` then you will need to add references to `Topshelf.dll`

Compiling From Source
'''''''''''''''''''''

Lastly, if you want to hack on Topshelf or just want to have the actual source
code you can clone the source from github.com.

To clone the repository using git try the following::

    git clone git://github.com/Topshelf/Topshelf.git

If you want the development branch (where active development happens)::

    git clone git://github.com/Topshelf/Topshelf.git
    cd Topshelf
    git checkout develop

Build Dependencies
''''''''''''''''''

To compile Topshelf from source you will need the following developer tools
installed:

 * .Net 4.0 sdk
 * ruby v 1.8.7
 * gems (rake, albacore)

Compiling
'''''''''

To compile the source code, drop to the command line and type::

    .\build.bat

If you look in the ``.\build_output`` folder you should see the binaries.
