using System.Configuration;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    public class TaskExecutorClient : BaseClient
    {
        private readonly PushClient ServiceBusProducer;

        public TaskExecutorClient(string TaskEventSocket, string ServiceBusClientSocket) : base(
            new SubscriberClient($">{TaskEventSocket}")
        )
        {
            ServiceBusProducer = new($">{ServiceBusClientSocket}");
        }

        public TaskExecutorClient(ConnectionStringSettingsCollection Connections) : base(
            new SubscriberClient($">{Connections["TaskEventSocket"].ConnectionString}")
        )
        {
            ServiceBusProducer = new($">{Connections["ServiceBusClientSocket"].ConnectionString}");
        }

        public void PublishReceipt(TaskEvent Task)
        {
            ServiceBusProducer.SendMQMessage(Task.GenerateReceipt(EnumTaskEventProcessingState.Processed).ToNetMQMessage());
        }
    }
}
