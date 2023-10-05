using NetMQ;
using Transport.Messages;

namespace Transport.Client
{
    using MQMessageBuffer = Queue<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public interface ITransportClient
    {
        public void RegisterServiceEventCallback(string ServiceName, Callback? CallbackFunction);
        public void RegisterServiceEventCallback(CallbackDict EventTopicCallbacks);
        public MQMessageBuffer ProcessPendingEvents(int batchSize = 1);
        public Task PushEventAsync(IEvent Event);
        public void PushEvent(IEvent Event);
    }
}
