Logging Integration with Topshelf
=================================

By default, Topshelf uses a TraceSource for logging. This is part of the .NET framework, and thus does not introduce any additional dependencies. However, many applications use more advanced logging libraries, such as log4net or NLog. To support this, an extensible logging interface is used by Topshelf.

log4net Integration
-------------------

To enable logging via log4net, the Topshelf.Log4Net NuGet package is available. Once added to your project, configure Topshelf to use log4net via the configuration:

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.UseLog4Net();
    });

This will change the ``HostLogger`` to use log4net. There is an overload that allows a configuration file to be specified. If given, the filename will be resolved to the ApplicationBase folder and passed to log4net to configure the log appenders and levels.


NLog Integration
----------------

To enable logging via NLog, the Topshelf.NLog NuGet package is available. Once added to your project, configure Topshelf to use NLog via the configuration:

.. sourcecode:: csharp

    HostFactory.New(x =>
    {
        x.UseNLog();
    });

This will change the ``HostLogger`` to use NLog. An existing LogFactory can be passed as well, using an overload of the same method.

