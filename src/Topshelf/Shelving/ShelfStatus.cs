namespace Topshelf.Shelving
{
    using System;
    using System.Runtime.Remoting;
    using Magnum.Channels;

    public class ShelfStatus
    {
        private WcfUntypedChannel _shelfChannel = null;

        public Func<WcfUntypedChannel> ShelfChannelBuilder { private get; set; }
        public string ShelfName { get; set; }
        public ObjectHandle ObjectHandle { get; set; }
        public Shelf RemoteShelf { get; set; }
        public WcfUntypedChannel ShelfChannel
        {
            get { return _shelfChannel ?? (_shelfChannel = ShelfChannelBuilder()); }
        }
        public AppDomain AppDomain { get; set; }
        public ShelfState CurrentState { get; set; }
    }
}