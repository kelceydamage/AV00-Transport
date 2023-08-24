using NetMQ;

namespace Transport.Generics
{
    using MQMessageBuffer = List<(NetMQMessage, bool?)>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public interface ISubscriber
    {
        public void Subscribe(string Topic);
        public MQMessageBuffer CollectAndInvokeMQMessages(int batchSize, short FrameCount, CallbackDict? Callbacks = null);
    }
}
