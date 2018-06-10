Topshelf Configuration
======================

While the Quickstart gives you enough to get going, there are many more features available in Topshelf. The following details the configuration options available, and how to use them in Topshelf services.


Service Name
------------

Specify the base name of the service, as it is registered in the services control manager. This setting is optional and by default uses the namespace of the Program.cs file (well, basically, the calling assembly type namespace).

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.SetServiceName("MyService");
    });

It is recommended that service names not contains spaces or other whitespace characters.

.. warning::

Each service on the system must have a unique name. If you need to run multiple instances of the same service,
consider using the InstanceName command-line option when registering the service.


Service Description
-------------------

Specify the description of the service in the services control manager. This is optional and defaults to the service name.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.SetDescription("My First Topshelf Service");
    });


Display Name
------------

Specify the display name of the service in the services control manager. This is optional and defaults to the service name.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.SetDisplayName("MyService");
    });


Instance Name
-------------

Specify the instance name of the service, which is combined with the base service name and separated by a $. This is optional, and is only added if specified.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.SetInstanceName("MyService");
    });

This option is typically set using the command-line argument, but it allowed here for completeness.


Service Configuration
=====================

The service can be configured in multiple ways, each with different goals. For services that can handle a dependency on Topshelf, the ``ServiceControl`` interface provides a lot of value for implementing the service control methods. Additionally, a zero-dependency solution is also available when lambda methods can be used to call methods in the service class.


Simple Service
--------------

To configure a simple service, the easiest configuration method is available.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.Service<MyService>();
    });

    // Service implements the ServiceControl methods directly and has a default constructor
    class MyService : ServiceControl
    {}

If the service does not have a default constructor, the constructor can be specified, allowing the service to be created by the application, such as when a container needs to be used.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.Service<MyService>(() => ObjectFactory.GetInstance<MyService>());
    });

    // Service implements the ServiceControl methods directly and has a default constructor
    class MyService : ServiceControl
    {
        public MyService(SomeDependency dependency)
        {}
    }

If the service needs access to the HostSettings during construction, they are also available as an overload.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.Service<MyService>(hostSettings => new MyService(hostSettings));
    });

    // Service implements the ServiceControl methods directly and has a default constructor
    class MyService : ServiceControl
    {
        public MyService(HostSettings settings)
        {}
    }


Custom Service
--------------

To configure a completely custom service, such as one that has no dependencies on Topshelf, the following configuration is available.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.Service<MyService>(sc =>
        {
            sc.ConstructUsing(() => new MyService());

            // the start and stop methods for the service
            sc.WhenStarted(s => s.Start());
            sc.WhenStopped(s => s.Stop());

            // optional pause/continue methods if used
            sc.WhenPaused(s => s.Pause());
            sc.WhenContinued(s => s.Continue());

            // optional, when shutdown is supported
            sc.WhenShutdown(s => s.Shutdown());
        });
    });

Each of the WhenXxx methods can also take an argument of the ``HostControl`` interface, which can be used to request the service be stopped, request additional start/stop time, etc.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.Service<MyService>(sc =>
        {
            sc.WhenStarted((s, hostControl) => s.Start(hostControl));
        }
    }

The ``HostControl`` interface can be retained and used as the service is running to Stop the service.


Service Start Modes
===================

There are multiple service start modes, each of which can be specified by the configuration. This option is only used if the service is being installed.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.StartAutomatically(); // Start the service automatically
        x.StartAutomaticallyDelayed(); // Automatic (Delayed) -- only available on .NET 4.0 or later
        x.StartManually(); // Start the service manually
        x.Disabled(); // install the service as disabled
    });

Service Recovery
================

Topshelf also exposes the options needed to configure the service recovery options as well.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.EnableServiceRecovery(r =>
        {
            //you can have up to three of these
            r.RestartComputer(5, "message");
            r.RestartService(0);
            //the last one will act for all subsequent failures
            r.RunProgram(7, "ping google.com");

            //should this be true for crashed or non-zero exits
            r.OnCrashOnly();

            //number of days until the error count resets
            r.SetResetPeriod(1);
        });
    });

Service Identity
================

Services can be configured to run as a number of different identities, using the configuration option that is most appropriate.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.RunAs("username", "password");
    });

Runs the service using the specified username and password. This can also be configured using the command-line. 
Please be sure to include the domain or UPN suffix in the username value e.g. **domain\\username** or **username@suffix.com**.


.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.RunAsPrompt();
    });

When the service is installed, the installer will prompt for the username/password combination used to launch the service.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.RunAsNetworkService();
    });

Runs the service using the NETWORK_SERVICE built-in account.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.RunAsLocalSystem();
    });

Runs the service using the local system account.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.RunAsLocalService();
    });

Runs the service using the local service account.


Custom Install Actions
======================

These actions allow user-specified code to be executed during the service install/uninstall process. Each install action takes a *settings* parameter of type Topshelf.HostSettings, providing you with an API to service-related properties such as the *InstanceName*, *ServiceName*, etc.

Before Install Actions
----------------------

Topshelf allows actions to be specified that are executed before the service is installed. Note that this action is only executed if the service is being installed.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.BeforeInstall(settings => { ... });
    });


After Install Actions
---------------------

Topshelf allows actions to be specified that are executed after the service is installed. Note that this action is only executed if the service is being installed.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.AfterInstall(settings => { ... });
    });

Before Uninstall Actions
------------------------

Topshelf allows actions to be specified that are executed before the service is uninstalled. Note that this action is only executed if the service is being uninstalled.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.BeforeUninstall(() => { ... });
    });


After Uninstall Actions
-----------------------

Topshelf allows actions to be specified that are executed after the service is uninstalled. Note that this action is only executed if the service is being uninstalled.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.AfterUninstall(() => { ... });
    });


Service Dependencies
====================

Service dependencies can be specified such that the service does not start until the dependent services are started. This is managed by the windows services control manager, and not by Topshelf itself.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.DependsOn("SomeOtherService");
    });

There are a number of built-in extension methods for well-known services, including:

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.DependsOnMsmq(); // Microsoft Message Queueing
        x.DependsOnMsSql(); // Microsoft SQL Server
        x.DependsOnEventLog(); // Windows Event Log
        x.DependsOnIis(); // Internet Information Server
    });


Advanced Settings
=================

EnablePauseAndContinue
----------------------


Specifies that the service supports pause and continue, allowing the services control manager to pass pause and continue commands to the service.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.EnablePauseAndContinue();
    });


EnableShutdown
--------------

Specifies that the service supports the shutdown service command, allowing the services control manager to quickly shutdown the service.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.EnableShutdown();
    });


OnException
--------------

Provides a callback for exceptions that are thrown while the service is running. This callback is not a handler, and will not affect the default exception handling that Topshelf already provides. It is intended to provide visibility into thrown exceptions for triggering external actions, logging, etc.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.OnException(ex =>
        {
            // Do something with the exception
        });
    });


Service Recovery
================

To configure the service recovery options, a configurator is available to specify one or more service recovery actions. The recovery options are only used when installing the service, and are set once the service has been successfully installed.

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.EnableServiceRecovery(rc =>
        {
            rc.RestartService(1); // restart the service after 1 minute
            rc.RestartSystem(1, "System is restarting!"); // restart the system after 1 minute
            rc.RunProgram(1, "notepad.exe"); // run a program
            rc.SetResetPeriod(1); // set the reset interval to one day
        })
    });

The recovery actions are executed in the order specified, with the next action being executed after the previous action was run and the service failed again. There is a limit (based on the OS) of how many actions can be executed, and is typically 2-3 actions.










