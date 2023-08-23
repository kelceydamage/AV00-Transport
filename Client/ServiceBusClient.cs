using NetMQ;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    using MQMessageBuffer = List<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class ServiceBusClient
    {
        private readonly SubscriberClient subscriberClient;
        private readonly PushClient pushClient;
        private readonly CallbackDict EventTopicCallbacks = new();
        public string[]? SubscribedTopics;

        public ServiceBusClient(string PushSocketAddress, string SubscriberSocketAddress)
        {
            pushClient = new PushClient(PushSocketAddress);
            subscriberClient = new SubscriberClient(SubscriberSocketAddress);
        }

        public void PushTask(TaskEvent Task)
        {
            pushClient.SendMQMessage(Task.ToNetMQMessage());
        }

        public void RegisterEventTopicAndCallback(string TopicName, Callback? CallbackFunction)
        {
            if (!EventTopicCallbacks.ContainsKey(TopicName) && CallbackFunction is not null)
                EventTopicCallbacks.Add(TopicName, CallbackFunction);
            subscriberClient.Subscribe(TopicName);
        }

        public void RegisterEventTopicAndCallback(CallbackDict EventTopicCallbacks)
        {
            foreach (KeyValuePair<string, Callback> EventTopicCallback in EventTopicCallbacks)
                RegisterEventTopicAndCallback(EventTopicCallback.Key, EventTopicCallback.Value);
        }

        public MQMessageBuffer CollectEventReceipts(int batchSize = 1)
        {
            return subscriberClient.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
        }
    }
}
