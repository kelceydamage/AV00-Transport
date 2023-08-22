using NetMQ;
using Transport.Sockets;

namespace Transport.Client
{
    public class ServiceBusClient
    {
        Subscriber SubscriberSocket;
        Push PushSocket;
        private Dictionary<string, Func<NetMQMessage, bool>> EventTopicCallbacks = new Dictionary<string, Func<NetMQMessage, bool>>();
        public string[]? SubscribedTopics;

        public ServiceBusClient(string PushSocketAddress, string SubscriberSocketAddress)
        {
            PushSocket = new Push(PushSocketAddress);
            SubscriberSocket = new Subscriber(SubscriberSocketAddress);
        }

        public void SubscribeForUpdates(string[] Topics)
        {
            foreach (string topic in Topics)
                SubscriberSocket.Subscribe(topic);
            SubscribedTopics = Topics;
        }

        public void SubscribeForUpdates(string Topic)
        {
            SubscriberSocket.Subscribe(Topic);
            SubscribedTopics = new string[] { Topic };
        }

        public void PushTask(string Message)
        {
            PushSocket.SendFrame(Message);
        }

        public void RegisterEventTopicCallback(string TopicName, Func<NetMQMessage, bool> CallbackFunction)
        {
            if (!EventTopicCallbacks.ContainsKey(TopicName))
                EventTopicCallbacks.Add(TopicName, CallbackFunction);
        }

        public List<(NetMQMessage, bool?)> CollectEvents(int BatchSize = 1)
        {
            return CollectEventsByBatch(BatchSize);
        }

        // If there are pending messages, collect [BatchSize] number and return them.
        private List<(NetMQMessage, bool?)> CollectEventsByBatch(int batchSize)
        {
            List<(NetMQMessage, bool?)> collectedEvents = new();
            var message = new NetMQMessage();
            for (int count = 0; count < batchSize; count++)
            {
                if (!SubscriberSocket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(500), message: ref message, 2))
                {
                    Console.WriteLine($"No events found");
                    break;
                }
                EventTopicCallbacks.TryGetValue(message[0].ConvertToString(), out Func<NetMQMessage, bool>? callback);
                // TODO: Make async
                collectedEvents.Add((message, callback?.Invoke(message)));
            }
            return collectedEvents;
        }
    }
}
