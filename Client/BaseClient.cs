using NetMQ;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    using MQMessageBuffer = List<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class BaseClient
    {
        protected readonly CallbackDict EventTopicCallbacks = new();
        internal readonly SubscriberClient Subscriber;

        public BaseClient(string EventSocket)
        {
            Subscriber = new SubscriberClient(EventSocket);
        }

        public void RegisterServiceEventCallback(string ServiceName, Callback? CallbackFunction)
        {
            if (!EventTopicCallbacks.ContainsKey(ServiceName) && CallbackFunction is not null)
                EventTopicCallbacks.Add(ServiceName, CallbackFunction);
            Subscriber.Subscribe(ServiceName);
        }

        public void RegisterEventTopicAndCallback(CallbackDict EventTopicCallbacks)
        {
            foreach (KeyValuePair<string, Callback> EventTopicCallback in EventTopicCallbacks)
                RegisterServiceEventCallback(EventTopicCallback.Key, EventTopicCallback.Value);
        }

        public MQMessageBuffer ProcessPendingEvents(int batchSize = 1)
        {
            return Subscriber.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
        }
    }
}
