using NetMQ;
using System;

namespace Transport.Messages
{
    public class TransportMessage : NetMQMessage { }

    public class BaseEvent
    {
        public virtual TransportMessage Serialize()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Deserialize(TransportMessage WireMessage)
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
