using NetMQ;
using Transport.Messages;
using Transport.Generics;
using System.Configuration;
using System.Collections.Specialized;

namespace Transport.Relay
{
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class ServiceBusRelay
    {
        private readonly PublisherClient receiptPublisher;
        private readonly PublisherClient taskPublisher;
        private readonly PullClient taskReceiver;
        private readonly CallbackDict EventTopicCallbacks = new();
        private readonly int batchSize;
        private readonly bool issueReceipts = false;
        private readonly bool enableDebugLogging = false;

        public ServiceBusRelay(string TaskReceiveSocketAddress, string ReceiptPublisherSocketAddress, string TaskPublisherSocketAddress, int BatchSize)
        {
            receiptPublisher = new PublisherClient(ReceiptPublisherSocketAddress);
            taskPublisher = new PublisherClient(TaskPublisherSocketAddress);
            taskReceiver = new PullClient(TaskReceiveSocketAddress);
            EventTopicCallbacks.Add("MyTaskStream", ReceiveMessageHandlerCallback);
            batchSize = BatchSize;
        }

        public ServiceBusRelay(ConnectionStringSettingsCollection Connections, NameValueCollection Settings)
        {
            receiptPublisher = new PublisherClient(Connections["OutboundTaskReceipts"].ConnectionString);
            taskPublisher = new PublisherClient(Connections["OutboundTasks"].ConnectionString);
            taskReceiver = new PullClient(Connections["InboundTasks"].ConnectionString);
            EventTopicCallbacks.Add("MyTaskStream", ReceiveMessageHandlerCallback);
            batchSize = int.Parse(Settings["InboundTaskCollectionBatchSize"] ?? throw new Exception());
            issueReceipts = bool.Parse(Settings["IssueReceipts"] ?? throw new Exception());
            enableDebugLogging = bool.Parse(Settings["EnableDebugLogging"] ?? throw new Exception());
        }

        public void ForwardMessage()
        {
            taskReceiver.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
        }

        private bool ReceiveMessageHandlerCallback(NetMQMessage MQMessage)
        {
            if (enableDebugLogging)
            {
                Console.WriteLine($"RELAY: Received serviceName={MQMessage[0].ConvertToString()}");
                Console.WriteLine($"RELAY: Received type={MQMessage[1].ConvertToString()}");
                Console.WriteLine($"RELAY: Received guid={MQMessage[2].ConvertToString()}");
                Console.WriteLine($"RELAY: Received data={MQMessage[3].ConvertToString()}");
            }
            string eventTypeName = MQMessage[1].ConvertToString();
            switch(Enum.Parse<EnumEventType>(eventTypeName))
            {
                case EnumEventType.TaskEvent:
                    ForwardTask(MQMessage, issueReceipts);
                    break;
                case EnumEventType.TaskEventReceipt:
                    receiptPublisher.SendMQMessage(MQMessage);
                    break;
                default:
                    throw new Exception($"Unknown Event Type: {eventTypeName}");
            }
            return true;
        }

        private void ForwardTask(NetMQMessage MQMessage, bool IssueReceipts)
        {
            taskPublisher.SendMQMessage(MQMessage);
            if (IssueReceipts)
            {
                TaskEvent currentTask = new();
                currentTask.FromNetMQMessage(MQMessage);
                receiptPublisher.SendMQMessage(currentTask.GenerateReceipt(EnumTaskEventProcessingState.Processing).ToNetMQMessage());
            }
        }
    }
}
