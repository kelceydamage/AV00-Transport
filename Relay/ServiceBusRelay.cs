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
        private readonly int batchDelayMs = 0;
        private readonly bool issueReceipts = false;
        private readonly bool enableDebugLogging = false;

        public ServiceBusRelay(string TaskBusRelaySocket, string ReceiptEventSocket, string TaskEventSocket, int BatchSize)
        {
            receiptPublisher = new PublisherClient(ReceiptEventSocket);
            taskPublisher = new PublisherClient(TaskEventSocket);
            taskReceiver = new PullClient(TaskBusRelaySocket);
            EventTopicCallbacks.Add("MyTaskStream", ReceiveMessageHandlerCallback);
            batchSize = BatchSize;
        }

        public ServiceBusRelay(ConnectionStringSettingsCollection Connections, NameValueCollection Settings)
        {
            receiptPublisher = new PublisherClient(Connections["ReceiptEventSocket"].ConnectionString);
            taskPublisher = new PublisherClient(Connections["TaskEventSocket"].ConnectionString);
            taskReceiver = new PullClient(Connections["TaskBusRelaySocket"].ConnectionString);
            EventTopicCallbacks.Add("DEFAULT", ReceiveMessageHandlerCallback);
            batchSize = int.Parse(Settings["RelayInboundTaskCollectionBatchSize"] ?? throw new Exception());
            issueReceipts = bool.Parse(Settings["RelayIssueReceipts"] ?? throw new Exception());
            enableDebugLogging = bool.Parse(Settings["RelayEnableDebugLogging"] ?? throw new Exception());
            batchDelayMs = int.Parse(Settings["RelayInboundTaskCollectionBatchDelayMs"] ?? throw new Exception());
        }

        public void ForwardMessage()
        {
            taskReceiver.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
        }

        public void ForwardMessages()
        {
            while (true)
            {
                taskReceiver.CollectAndInvokeMQMessages(batchSize, IEvent.FrameCount, EventTopicCallbacks);
                Thread.Sleep(batchDelayMs);
            }
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
