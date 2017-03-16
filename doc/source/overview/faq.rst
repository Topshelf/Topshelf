Topshelf Key Concepts
=====================

Why would I want to use Topshelf?
---------------------------------

Topshelf is a Windows service framework for the .NET platform. Topshelf makes it easy to create a Windows service, test the service, debug the service, and ultimately install it into the Windows Service Control Manager (SCM). Topshelf does this by allowing developers to focus on service logic instead of the details of interacting with the built-in service support in the .NET framework. Developers don’t need to understand the complex details of service classes, perform installation via InstallUtil, or learn how to attach the debugger to services for troubleshooting issues.

How does Topshelf do this?
--------------------------

When a developer uses Topshelf, creating a Windows service is as easy as creating a console application. Once the console application is created, the developer creates a single service class that has public Start and Stop methods. With a few lines of configuration using Topshelf’s configuration API, the developer has a complete Windows service that can be debugged using the debugger (yes, F5 debugging support for services) and installed using the Topshelf command-line support.

What if my service requires custom installation options?
---------------------------

Topshelf supports most of the commonly used service installation options, including:

* Automatic, Automatic (Delayed), Manual, and Disabled start options. 
* Local System, Local Service, Network Service, Username/Password, or prompted service credentials during installation. 
* Service start dependencies, including SQL Server, MSMQ, and others.  
* Multiple instance service installation with distinct service names. 
* Service Recovery options, including restart, reboot, or run program. 

What’s the impact to my service?
----------------------------

Topshelf is a small (around 200 KB) assembly with no dependencies, making it easy to integrate with any service.

What about command-line arguments?
----------------------------

Topshelf has an extensible command-line, allowing services to register parameters that can be specified using command-line arguments.

What else can I get by using Topshelf?
----------------------------

Topshelf is an open-source project, and new contributions are being accepted regularly. While the base Topshelf assembly is being kept small with very specific functionality, additional assemblies that build on top of Topshelf are also being created. For example, a supervisory extension that monitors service conditions such as CPU and memory is being created that will automatically cycle the service if conditions exceed specifications. This reduces the need to closely monitor services and manually restart them when they misbehave.

Is Topshelf just for Windows?
----------------------

Topshelf works with Mono, making it possible to deploy services to Linux. The service installation features are currently Windows only, but others are working on creating native host environment support so that installation and management features are available as well.

