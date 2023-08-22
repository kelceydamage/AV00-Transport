using NetMQ.Sockets;

namespace Transport.Sockets
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
}
