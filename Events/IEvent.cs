using NetMQ;

namespace Transport.Event
{
    public interface IEvent
    {
        public const short FrameCount = 4;
        public EnumEventType Type { get; }
        public NetMQMessage Serialize();
    }
}
