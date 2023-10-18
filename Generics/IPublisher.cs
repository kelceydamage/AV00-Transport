using NetMQ;

namespace Transport.Generics
{
    public interface IPublisher
    {
        public void SendMQMessage(NetMQMessage message);
    }
}
