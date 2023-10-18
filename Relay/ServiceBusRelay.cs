﻿using NetMQ;
using Transport.Event;
using Transport.Generics;
using System.Configuration;
using System.Collections.Specialized;

namespace Transport.Relay
{
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class ServiceBusRelay
    {
        private readonly PublisherClient receiptPublisher;
        private readonly BoundPublisherClient eventPublisher;
        private readonly PullClient ServiceBusReceiver;
        private readonly CallbackDict EventTopicCallbacks = new();
        private readonly int batchSize;
        private readonly int batchDelayMs = 0;
        private readonly bool enableDebugLogging = false;
        private readonly short transportMessageFrameCount;

        public ServiceBusRelay(string ServiceBusServerSocket, string ReceiptEventSocket, string TaskEventSocket, int BatchSize)
        {
            receiptPublisher = new PublisherClient(ReceiptEventSocket);
            eventPublisher = new BoundPublisherClient(TaskEventSocket);
            ServiceBusReceiver = new PullClient(ServiceBusServerSocket);
            EventTopicCallbacks.Add("MyTaskStream", ReceiveMessageHandlerCallback);
            batchSize = BatchSize;
        }

        public ServiceBusRelay(ConnectionStringSettingsCollection Connections, NameValueCollection Settings)
        {
            receiptPublisher = new PublisherClient($"{Connections["ReceiptEventSocket"].ConnectionString}");
            eventPublisher = new BoundPublisherClient($"@{Connections["TaskEventSocket"].ConnectionString}");
            ServiceBusReceiver = new PullClient(Connections["ServiceBusServerSocket"].ConnectionString);
            EventTopicCallbacks.Add("DEFAULT", ReceiveMessageHandlerCallback);
            transportMessageFrameCount = short.Parse(Settings["TransportMessageFrameCount"] ?? throw new Exception());
            batchSize = int.Parse(Settings["RelayInboundTaskCollectionBatchSize"] ?? throw new Exception());
            enableDebugLogging = bool.Parse(Settings["RelayEnableDebugLogging"] ?? throw new Exception());
            batchDelayMs = int.Parse(Settings["RelayInboundTaskCollectionBatchDelayMs"] ?? throw new Exception());
        }

        public void ForwardMessage()
        {
            ServiceBusReceiver.CollectAndInvokeMQMessages(batchSize, transportMessageFrameCount, EventTopicCallbacks);
        }

        public void ForwardMessages()
        {
            while (true)
            {
                ServiceBusReceiver.CollectAndInvokeMQMessages(batchSize, transportMessageFrameCount, EventTopicCallbacks);
                Thread.Sleep(batchDelayMs);
            }
        }

        private bool ReceiveMessageHandlerCallback(NetMQMessage MQMessage)
        {
            if (enableDebugLogging)
            {
                Console.WriteLine($"RELAY: [Received] serviceName={MQMessage[0].ConvertToString()}");
                Console.WriteLine($"RELAY: [Received] type={MQMessage[1].ConvertToString()}");
                Console.WriteLine($"RELAY: [Received] guid={MQMessage[2].ConvertToString()}");
                Console.WriteLine($"RELAY: [Received] data={MQMessage[3].ConvertToString()}");
            }
            string eventTypeName = MQMessage[1].ConvertToString();
            switch(Enum.Parse<EnumEventType>(eventTypeName))
            {
                case EnumEventType.Event:
                    Console.WriteLine($"RELAY: [Forwarding] Event for event: {MQMessage[2].ConvertToString()}");
                    eventPublisher.SendMQMessage(MQMessage);
                    break;
                case EnumEventType.EventReceipt:
                    Console.WriteLine($"RELAY: [Forwarding] EventReceipt for event: {MQMessage[2].ConvertToString()}");
                    receiptPublisher.SendMQMessage(MQMessage);
                    break;
                default:
                    throw new Exception($"Unknown Event Type: {eventTypeName}");
            }
            return true;
        }
    }
}
