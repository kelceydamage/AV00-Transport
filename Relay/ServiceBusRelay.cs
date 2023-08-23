using NetMQ;
using Transport.Messages;
using Transport.Generics;

namespace Transport.Relay
{
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class ServiceBusRelay
    {
        private readonly PublisherClient publisherClient;
        private readonly PullClient pullClient;
        private readonly CallbackDict EventTopicCallbacks = new();
        private readonly int batchSize;

        public ServiceBusRelay(string PullSocketAddress, string PublisherSocketAddress, int BatchSize)
        {
            publisherClient = new PublisherClient(PublisherSocketAddress);
            pullClient = new PullClient(PullSocketAddress);
            EventTopicCallbacks.Add("MyTaskStream", DebugCallback);
            batchSize = BatchSize;
        }

        public void ForwardMessage()
        {
            pullClient.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
        }

        private bool DebugCallback(NetMQMessage MQMessage)
        {
            Console.WriteLine("RELAY: In Callback");
            TaskEvent MyTask = new();
            MyTask.FromNetMQMessage(MQMessage);
            Console.WriteLine($"RELAY: Received {MyTask.Topic}-{MyTask.TaskId}");
            if (MyTask.Data is not null)
                foreach (KeyValuePair<string, object> data in MyTask.Data)
                    Console.WriteLine($"RELAY: Received {data.Key}: {data.Value}");
            TaskEventReceipt MyTaskReceipt = new(MyTask.Topic, MyTask.TaskId, EnumTaskEventProcessingState.Processed);
            publisherClient.SendMQMessage(MyTaskReceipt.ToNetMQMessage());
            return true;
        }
    }
}
