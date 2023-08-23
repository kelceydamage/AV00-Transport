using System;
using System.ServiceModel.Channels;
using NetMQ;
using Transport.Client;
using Transport.Relay;
using Transport.Messages;

namespace HelloWorldDemo
{
    class Program
    {
        public static bool Callback(NetMQMessage MQMessage)
        {
            TaskEventReceipt MyTaskReceipt = new();
            MyTaskReceipt.FromNetMQMessage(MQMessage);
            Console.WriteLine("* Inside Callback");
            Console.WriteLine($"* From Server: topic={MyTaskReceipt.Topic}, State={MyTaskReceipt.ProcessingState}");
            Console.WriteLine($"* Frame count={MQMessage.FrameCount}-MessageLength={TaskEventReceipt.MessageLength}");
            return true;
        }

        static void Main(string[] args)
        {
            Console.Title = "NetMQ HelloWorld";

            var Bus = new ServiceBusRelay("@tcp://localhost:5556", "tcp://localhost:5557");
            var BusClient = new ServiceBusClient("tcp://localhost:5556", "tcp://localhost:5557");
            TaskEvent MyTask = new TaskEvent("MyTaskStream", "Hello World");

            BusClient.RegisterEventTopicAndCallback("MyTaskStream", Callback);
            BusClient.PushTask(MyTask);

            Bus.ReceiveFrameString();

            List<(NetMQMessage, bool?)> collectedEventReceipts = BusClient.CollectEventReceipts();
            foreach ((NetMQMessage, bool?) message in collectedEventReceipts)
            {
                Console.WriteLine($"Callback Processed: {message.Item2}");
            }

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();

            NetMQConfig.Cleanup();
        }
    }
}
