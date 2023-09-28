using NetMQ;
using Transport.Messages;

namespace Transport.Generics
{
    using MQMessageBuffer = List<(TransportMessage, bool?)>;
    using CallbackDict = Dictionary<string, Func<TransportMessage, bool>>;

    public interface ISubscriber
    {
        public void Subscribe(string Topic);
        public MQMessageBuffer CollectAndInvokeMQMessages(int batchSize, short FrameCount, CallbackDict? Callbacks = null);
    }
}
