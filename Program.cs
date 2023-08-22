using System;
using NetMQ;
using Transport.Client;
using Transport.Relay;

namespace HelloWorldDemo
{
    class Program
    {
        public static bool Callback(NetMQMessage message)
        {
            Console.WriteLine("* Inside Callback");
            Console.WriteLine($"* From Server: topic={message[0].ConvertToString()}, message={message[1].ConvertToString()}");
            Console.WriteLine($"* Frame count={message.FrameCount}");
            return true;
        }

        static void Main(string[] args)
        {
            Console.Title = "NetMQ HelloWorld";

            var Bus = new ServiceBusRelay("@tcp://localhost:5556", "tcp://localhost:5557");
            var BusClient = new ServiceBusClient("tcp://localhost:5556", "tcp://localhost:5557");
            BusClient.SubscribeForUpdates("test");
            BusClient.RegisterEventTopicCallback("test", Callback);
            BusClient.PushTask("Hello");

            Bus.ReceiveFrameString();
            List<(NetMQMessage, bool?)> CollectedEvents = BusClient.CollectEvents();
            foreach ((NetMQMessage, bool?) message in CollectedEvents)
            {
                Console.WriteLine($"Callback Processed: {message.Item2}");
                Console.WriteLine($"From Server: topic={message.Item1[0].ConvertToString()}, message={message.Item1[1].ConvertToString()}");
                Console.WriteLine($"Frame count={message.Item1.FrameCount}");
            }
            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();

            NetMQConfig.Cleanup();
        }
    }
}
