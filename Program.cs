using NetMQ;
using System.Configuration;
using Transport.Generics;
using Transport.Relay;

namespace Transport
{

    class Program
    {
        public readonly short FrameCount = 4;

        public static bool Callback(NetMQMessage MQMessage)
        {
            Console.WriteLine("* Inside Callback");
            Console.WriteLine($"* From Server: RawMessage={MQMessage}");
            Console.WriteLine($"* Frame count=4, MessageLength={MQMessage.FrameCount}");
            return true;
        }

        static void Main()
        {
            Console.Title = "NetMQ Transport Relay";

            ServiceBusRelay Bus = new(ConfigurationManager.ConnectionStrings, ConfigurationManager.AppSettings);

            SubscriberClient subscriber = new(ConfigurationManager.ConnectionStrings["TaskEventSocket"].ConnectionString);

            PushClient BusClient = new(ConfigurationManager.ConnectionStrings["ServiceBusClientSocket"].ConnectionString);

            NetMQMessage MyTask = new();
            MyTask.Append("Test");

            BusClient.SendMQMessage(MyTask);

            Bus.ForwardMessage();

            subscriber.Subscribe("MyTaskStream");
            subscriber.CollectAndInvokeMQMessages(5, 4, null);

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();

            NetMQConfig.Cleanup();
        }
    }
}
