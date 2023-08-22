using NetMQ;
using Transport.Messages;
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

        public void PushTask(TaskEvent Task)
        {
            PushSocket.SendMultipartMessage(Task.ToNetMQMessage());
        }

        public void RegisterEventTopicAndCallback(string TopicName, Func<NetMQMessage, bool>? CallbackFunction)
        {
            if (!EventTopicCallbacks.ContainsKey(TopicName) && CallbackFunction is not null)
                EventTopicCallbacks.Add(TopicName, CallbackFunction);
            SubscriberSocket.Subscribe(TopicName);
        }

        public void RegisterEventTopicAndCallback(Dictionary<string, Func<NetMQMessage, bool>?> EventTopicCallbacks)
        {
            foreach (KeyValuePair<string, Func<NetMQMessage, bool>?> EventTopicCallback in EventTopicCallbacks)
                RegisterEventTopicAndCallback(EventTopicCallback.Key, EventTopicCallback.Value);
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
                if (!SubscriberSocket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(500), message: ref message, 4))
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
