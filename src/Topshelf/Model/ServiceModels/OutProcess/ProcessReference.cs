namespace Topshelf.Model
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting;
    using Logging;
    using Magnum.Extensions;
    using Stact;


    public class ProcessReference :
        IDisposable
    {
        //TODO: can ShelfReference and ProcessReference be the same thing?
        static readonly ILog _log = Logger.Get("Topshelf.Model.ProcessReference");
        readonly UntypedChannel _controllerChannel;
        readonly AppDomainSetup _domainSettings;

        readonly string _serviceName;
        readonly ShelfType _shelfType;
        OutboundChannel _channel;
        bool _disposed;
        HostChannel _hostChannel;
        ObjectHandle _objectHandle;
        Process _processHandle;

        public void Send<T>(T message)
        {
            if (_channel == null)
            {
                _log.WarnFormat("Unable to send service message due to null channel, service = {0}, message type = {1}",
                                _serviceName, typeof(T).ToShortTypeName());
                return;
            }

            _channel.Send(message);
        }

        public void Unload()
        {
            if(_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
        }

        public void Create()
        {
            var psi = new ProcessStartInfo("name", "shelf -port:22?");
            _processHandle = Process.Start(psi);
        
        }

        public void CreateShelfChannel(Uri uri, string pipeName)
        {
            _log.DebugFormat("[{0}] Creating shelf proxy: {1} ({2})", _serviceName, uri, pipeName);

            _channel = new OutboundChannel(uri, pipeName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if(disposing)
            {
                if(_hostChannel != null)
                {
                    _hostChannel.Dispose();
                    _hostChannel = null;
                }

                if(_channel != null)
                {
                    _channel.Dispose();
                    _channel = null;
                }

                _processHandle.Kill();
                _processHandle = null;
            }

            _disposed = true;
        }
    }
}