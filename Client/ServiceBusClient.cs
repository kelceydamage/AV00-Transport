using NetMQ;
using System.Collections.Specialized;
using System.Configuration;
using Transport.Generics;
using Transport.Messages;

namespace Transport.Client
{
    public class ServiceBusClient: BaseClient
    {
        private readonly PushClient TaskSender;

        public ServiceBusClient(string TaskBusClientSocket, string ReceiptEventSocket): base(ReceiptEventSocket)
        {
            TaskSender = new PushClient(TaskBusClientSocket);
        }

        public ServiceBusClient(ConnectionStringSettingsCollection Connections) : base(Connections["ReceiptEventSocket"].ConnectionString)
        {
            TaskSender = new PushClient(Connections["TaskBusClientSocket"].ConnectionString);
        }

        public void PushTask(TaskEvent Task)
        {
            TaskSender.SendMQMessage(Task.ToNetMQMessage());
        }
    }
}
