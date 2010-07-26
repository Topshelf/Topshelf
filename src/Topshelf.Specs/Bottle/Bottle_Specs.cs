namespace Topshelf.Specs.Bottle
{
    using System;
    using Magnum;
    using Magnum.Extensions;
    using Magnum.FileSystem;

    public class Bottle_Specs
    {
        public void Bob()
        {
            var n = new Bottles.BottleWatcher();
            var f = new Future<Directory>();
            n.Watch(".\\watch", f.Complete);

            System.IO.File.Copy(".\\bottle\\sample.zip",".\\watch\\sample.bottle");
            f.WaitUntilCompleted(5.Seconds());
            var d = f.Value;
            Console.WriteLine(d.Name.GetName());
            foreach (var file in d.GetFiles())
            {
                Console.WriteLine(file.Name.GetPath());
            }

        }
    }
}