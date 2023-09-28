using NetMQ;

namespace Transport.Messages
{
    public class BaseEvent
    {
        public virtual NetMQMessage Serialize()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Deserialize(NetMQMessage WireMessage)
        {
            throw new System.NotImplementedException();
        }
    }

    public enum EnumEventType
    {
        Event,
        EventReceipt
    }
}
