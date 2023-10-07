using NetMQ;
using Transport.Generics;

namespace Transport.Client
{
    using MQMessageBuffer = Queue<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class BaseTransportClient
    {
        protected readonly CallbackDict EventTopicCallbacks = new();
        internal readonly ISubscriber ServiceBusSubscriber;
        internal readonly short FrameCount;

        public BaseTransportClient(ISubscriber MQSubscriber, short MessageFrameCount)
        {
            ServiceBusSubscriber = MQSubscriber;
            FrameCount = MessageFrameCount;
        }

        public void RegisterServiceEventCallback(string ServiceName, Callback? CallbackFunction)
        {
            if (!EventTopicCallbacks.ContainsKey(ServiceName) && CallbackFunction is not null)
                EventTopicCallbacks.Add(ServiceName, CallbackFunction);
            ServiceBusSubscriber.Subscribe(ServiceName);
        }

        public void RegisterServiceEventCallback(CallbackDict EventTopicCallbacks)
        {
            foreach (KeyValuePair<string, Callback> EventTopicCallback in EventTopicCallbacks)
                RegisterServiceEventCallback(EventTopicCallback.Key, EventTopicCallback.Value);
        }

        public MQMessageBuffer ProcessPendingEvents(int batchSize = 1)
        {
            return ServiceBusSubscriber.CollectAndInvokeMQMessages(batchSize, FrameCount, EventTopicCallbacks);
        }

        public async Task<MQMessageBuffer> ProcessPendingEventsAsync(int batchSize = 1)
        {
            return await Task.FromResult(
                ServiceBusSubscriber.CollectAndInvokeMQMessages(batchSize, FrameCount, EventTopicCallbacks)
            );
        }
    }
}
