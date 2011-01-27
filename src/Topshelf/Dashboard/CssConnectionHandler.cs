namespace Topshelf.Dashboard
{
    using System.IO;
    using System.Linq;
    using Magnum.Extensions;
    using Stact;
    using Stact.ServerFramework;


    public class CssConnectionHandler :
        PatternMatchConnectionHandler
    {
        readonly CssChannel _statusChannel;

        public CssConnectionHandler() :
            base(".css$", "GET")
        {
            _statusChannel = new CssChannel();
        }

        protected override Channel<ConnectionContext> CreateChannel(ConnectionContext context)
        {
            return _statusChannel;
        }


        class CssChannel :
            Channel<ConnectionContext>
        {
            readonly Fiber _fiber;

            public CssChannel()
            {
                _fiber = new PoolFiber();
            }

            public void Send(ConnectionContext context)
            {
                _fiber.Add(() =>
                    {
                        string localPath = context.Request.Url.LocalPath;
                        string cssName = localPath.Split('/').Last();
                        context.Response.ContentType = "text/css";
                        using (Stream str = GetType().Assembly.GetManifestResourceStream("Topshelf.Dashboard.styles." + cssName))
                        {
                            byte[] buff = str.ReadToEnd();
                            context.Response.OutputStream.Write(buff, 0, buff.Length);
                        }
                        context.Complete();
                    });
            }
        }
    }
}