using System;
using NetMQ;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetMQ.Sockets;
using System.ServiceModel.Channels;
using System.Net.WebSockets;

namespace HelloWorldDemo
{
    public class Server : ResponseSocket
    {
        public Server(string? Socket) : base(Socket)
        {
            string serverSocket = Socket ?? "@tcp://localhost:5556";
        }
    }

    public class Client : RequestSocket
    {
        public Client(string? Socket) : base(Socket)
        {
            string clientSocket = Socket ?? "tcp://localhost:5556";
        }
    }

    public class Dealer : DealerSocket
    {
        public Dealer(string? Socket) : base(Socket)
        {
            string dealerSocket = Socket ?? "tcp://localhost:5556";
        }
    }

    public class Router : RouterSocket
    {
        public Router(string? Socket) : base(Socket)
        {
            string routerSocket = Socket ?? "tcp://localhost:5556";
        }
    }

    public class Publisher : PublisherSocket
    {
        public Publisher(string? Socket)
        {
            string publisherSocket = Socket ?? "tcp://localhost:5556";
            Bind(publisherSocket);
        }
    }

    public class Subscriber : SubscriberSocket
    {
        public Subscriber(string? Socket) : base(Socket)
        {
            string subscriberSocket = Socket ?? "tcp://localhost:5556";
        }
    }

    public class Push : PushSocket
    {
        public Push(string? Socket) : base(Socket)
        {
            string pushSocket = Socket ?? "tcp://localhost:5556";
        }
    }   

    public class Pull : PullSocket
    {
        public Pull(string? Socket) : base(Socket)
        {
            string pullSocket = Socket ?? "tcp://localhost:5556";
        }
    }   

    public class BusRelay
    {
        Publisher PublisherSocket;
        Pull PullSocket;

        public BusRelay(string? PullSocketAddress, string? PublisherSockerAddress)
        {
            PublisherSocket = new Publisher(PublisherSockerAddress);
            PullSocket = new Pull(PullSocketAddress);
        }

        public void Run()
        {
            while (true)
            {
                string message = PullSocket.ReceiveFrameString();
                Console.WriteLine("Relay Received {0}", message);
                PublisherSocket.SendFrame(message);
                Thread.Sleep(500);
            }
        }

        public void ReceiveFrameString()
        {
            string message = PullSocket.ReceiveFrameString();
            Console.WriteLine("Relay Received {0}", message);
            PublisherSocket.SendMoreFrame("test").SendFrame(message);
        }
    }

    public class BusClient
    {
        Subscriber SubscriberSocket;
        Push PushSocket;

        public BusClient(string PushSockerAddress, string SubscriberSocketAddress)
        {
            PushSocket = new Push(PushSockerAddress);
            SubscriberSocket = new Subscriber(SubscriberSocketAddress);
            SubscriberSocket.Subscribe("test");
        }

        public void SendFrame(string Message)
        {
            PushSocket.SendFrame(Message);
        }

        public string[] ReceiveFrameString()
        {
            string topic =  SubscriberSocket.ReceiveFrameString();
            string message = SubscriberSocket.ReceiveFrameString();
            return new string[] { topic, message };
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "NetMQ HelloWorld";

            var ServiceBus = new BusRelay("@tcp://localhost:5556", "tcp://localhost:5557");
            var ServiceBusClient = new BusClient("tcp://localhost:5556", "tcp://localhost:5557");

            ServiceBusClient.SendFrame("Hello");

            ServiceBus.ReceiveFrameString();
            string[] message = ServiceBusClient.ReceiveFrameString();


            Console.WriteLine($"From Server: topic={message[0]}, message={message[1]}");

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}