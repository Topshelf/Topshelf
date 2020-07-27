Show me the code!
=================

All right, all right, already. Here you go. Below is a functional setup of
Topshelf.

.. sourcecode:: csharp
    :linenos:

    public class TownCrier
    {
        readonly Timer _timer;
        public TownCrier()
        {
            _timer = new Timer(1000) {AutoReset = true};
            _timer.Elapsed += (sender, eventArgs) => Console.WriteLine("It is {0} and all is well", DateTime.Now);
        }
        public void Start() { _timer.Start(); }
        public void Stop() { _timer.Stop(); }
    }

    public class Program
    {
        public static void Main()
        {
            var rc = HostFactory.Run(x =>                                   //1
            {
                x.Service<TownCrier>(s =>                                   //2
                {
                   s.ConstructUsing(name=> new TownCrier());                //3
                   s.WhenStarted(tc => tc.Start());                         //4
                   s.WhenStopped(tc => tc.Stop());                          //5
                });
                x.RunAsLocalSystem();                                       //6

                x.SetDescription("Sample Topshelf Host");                   //7
                x.SetDisplayName("Stuff");                                  //8
                x.SetServiceName("Stuff");                                  //9
            });                                                             //10

            var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());  //11
            Environment.Exit(exitCode);
        }
    }


Now for the play by play.
"""""""""""""""""""""""""""""""""""
#. Here we are setting up the host using the ``HostFactory.Run``  the runner. We open up a new lambda where the ``x`` in this case exposes all of the host level configuration. Using this approach the command arguments are extracted from environment variables. We also capture the return code of the service - which we return on line 11.
#. Here we are telling Topshelf that there is a service of type ``TownCrier``. The lambda that gets opened here is exposing the service configuration options through the ``s`` parameter.
#. This tells Topshelf how to build an instance of the service. Currently we are just going to ‘new it up’ but we could just as easily pull it from an IoC container with some code that would look something like ``container.GetInstance<TownCrier>()``
#. How does Topshelf start the service
#. How does Topshelf stop the service
#. Here we are setting up the ‘run as’ and have selected the ‘local system’. We can also set up from the command line interactively with a win from type prompt and we can also just pass in some username/password as string arguments
#. Here we are setting up the description for the winservice to be use in the windows service monitor
#. Here we are setting up the display name for the winservice to be use in the windows service monitor
#. Here we are setting up the service name for the winservice to be use in the windows service monitor
#. Now that the lambda has closed, the configuration will be executed and the host will start running.
#. Finally, we convert and return the service exit code.

.. warning::
    You can only have ONE service! As of 3.x Topshelf the base product no longer
    support hosting multiple services. This was done because the code to implement
    was very brittle and hard to debug. We have opted for a simpler and cleaner
    base product. This feature will most likely come back in the form of an add
    on nuget.
