using System.Configuration;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    public class ServiceBusClient: BaseClient
    {
        private readonly PushClient ServiceBusProducer;

        public ServiceBusClient(string ServiceBusClientSocket, string ReceiptEventSocket) : base(
            new SubscriberClient($"{ReceiptEventSocket}")
        )
        {
            ServiceBusProducer = new PushClient(ServiceBusClientSocket);
        }

        public ServiceBusClient(ConnectionStringSettingsCollection Connections) : base(
            new SubscriberClient($"{Connections["ReceiptEventSocket"].ConnectionString}")
        )
        {
            ServiceBusProducer = new PushClient(Connections["ServiceBusClientSocket"].ConnectionString);
        }

        public void PushTask(TaskEvent Task)
        {
            ServiceBusProducer.SendMQMessage(Task.ToNetMQMessage());
        }
    }
}
