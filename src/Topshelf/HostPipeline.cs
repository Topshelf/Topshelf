namespace Topshelf
{
    using Magnum.Pipeline;
    using Magnum.Pipeline.Segments;

    public class HostPipeline
    {
        readonly Pipe _pipe;

        public HostPipeline()
        {
            _pipe = PipeSegment.New();
        }

        public void Send<T>(T message) where T : class
        {
            _pipe.Send(message);
        }

        public ISubscriptionScope NewSubscribeScope()
        {
            return _pipe.NewSubscriptionScope();
        }
    }
}