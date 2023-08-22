using NetMQ;
using Transport.Sockets;

namespace Transport.Relay
{
    public class ServiceBusRelay
    {
        Publisher PublisherSocket;
        Pull PullSocket;

        public ServiceBusRelay(string? PullSocketAddress, string? PublisherSockerAddress)
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
}
