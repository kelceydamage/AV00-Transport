using NetMQ;
using System.Collections.Specialized;
using System.Configuration;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    using MQMessageBuffer = List<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class ServiceBusClient
    {
        private readonly SubscriberClient Subscriber;
        private readonly PushClient TaskSender;
        private readonly CallbackDict EventTopicCallbacks = new();
        public string[]? SubscribedTopics;

        public ServiceBusClient(string TaskBusClientSocket, string ReceiptEventSocket)
        {
            TaskSender = new PushClient(TaskBusClientSocket);
            Subscriber = new SubscriberClient(ReceiptEventSocket);
        }

        public ServiceBusClient(ConnectionStringSettingsCollection Connections, NameValueCollection Settings)
        {
            TaskSender = new PushClient(Connections["TaskBusClientSocket"].ConnectionString);
            Subscriber = new SubscriberClient(Connections["ReceiptEventSocket"].ConnectionString);
        }

        public void PushTask(TaskEvent Task)
        {
            TaskSender.SendMQMessage(Task.ToNetMQMessage());
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
        
        public MQMessageBuffer CollectEventReceipts(int batchSize = 1)
        {
            return Subscriber.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
        }
    }
}
