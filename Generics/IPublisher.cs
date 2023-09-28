using NetMQ;
using Transport.Messages;

namespace Transport.Generics
{
    public interface IPublisher
    {
        public void SendMQMessage(TransportMessage message);
    }
}
