using System.Configuration;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    public class TaskExecutorClient : BaseClient
    {
        private readonly PublisherClient receiptPublisher;

        public TaskExecutorClient(string TaskEventSocket, string ReceiptEventSocket) : base(TaskEventSocket)
        {
            receiptPublisher = new PublisherClient(ReceiptEventSocket);
        }

        public TaskExecutorClient(ConnectionStringSettingsCollection Connections) : base(Connections["TaskEventSocket"].ConnectionString)
        {
            receiptPublisher = new PublisherClient(Connections["ReceiptEventSocket"].ConnectionString);
        }

        public void PublishReceipt(TaskEvent Task)
        {
            receiptPublisher.SendMQMessage(Task.GenerateReceipt(EnumTaskEventProcessingState.Processed).ToNetMQMessage());
        }
    }
}
