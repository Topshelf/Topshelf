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

        public void Dispose()
        {
            
        }

        public void Start(ProcessStartInfo psi)
        {
        }

        public void Unload()
        {
            
        }

        public void Create()
        {
            var psi = new ProcessStartInfo("name", "shelf -port:22?");
            _processHandle = Process.Start(psi);
        
        }
    }
}