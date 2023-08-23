using NetMQ;
using System.Configuration;
using Transport.Client;
using Transport.Relay;
using Transport.Messages;

namespace Transport
{
    using DataDict = Dictionary<string, object>;

    class Program
    {
        public static bool Callback(NetMQMessage MQMessage)
        {
            TaskEventReceipt MyTaskReceipt = new();
            MyTaskReceipt.FromNetMQMessage(MQMessage);
            Console.WriteLine("* Inside Callback");
            Console.WriteLine($"* From Server: topic={MyTaskReceipt.ServiceName}, State={MyTaskReceipt.ProcessingState}");
            Console.WriteLine($"* Frame count={MQMessage.FrameCount}-MessageLength={Event.GetFrameCountByEventType(MyTaskReceipt.EventType)}");
            return true;
        }

        static void Main()
        {
            Console.Title = "NetMQ Transport Relay";

            ServiceBusRelay Bus = new(ConfigurationManager.ConnectionStrings, ConfigurationManager.AppSettings);
            ServiceBusClient BusClient = new("tcp://localhost:5556", "tcp://localhost:5557");
            TaskEvent MyTask = new("MyTaskStream", new DataDict() { { "message", "Hello World" } });

            BusClient.RegisterServiceEventCallback(MyTask.ServiceName, Callback);
            BusClient.PushTask(MyTask);

            Bus.ForwardMessage();

            List<(NetMQMessage, bool?)> collectedEventReceipts = BusClient.CollectEventReceipts();
            foreach ((NetMQMessage, bool?) message in collectedEventReceipts)
                Console.WriteLine($"Callback Processed: {message.Item2}");

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();

            NetMQConfig.Cleanup();
        }
    }
}
