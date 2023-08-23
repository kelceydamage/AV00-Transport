using NetMQ;
using NetMQ.Sockets;

namespace Transport.Generics
{
    using MQMessageBuffer = List<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    internal class BaseClient
    {
        public BaseClient() { }

        protected static MQMessageBuffer CollectAndInvokeMQMessagesByBatch<T>(T Socket, int batchSize, short FrameCount, CallbackDict Callbacks) where T: NetMQSocket
        {
            MQMessageBuffer messageBuffer = new();
            var message = new NetMQMessage();
            for (int count = 0; count < batchSize; count++)
            {
                if (!Socket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(500), message: ref message, FrameCount))
                {
                    Console.WriteLine($"No events found");
                    break;
                }
                Callbacks.TryGetValue(message[0].ConvertToString(), out Callback? callback);
                // TODO: Make async
                messageBuffer.Add((message, callback?.Invoke(message)));
            }
            return messageBuffer;
        }

        protected static void SendMQMessage<T>(T Socket, NetMQMessage message) where T: NetMQSocket
        {
            Socket.SendMultipartMessage(message);
        }

        public virtual MQMessageBuffer CollectAndInvokeMQMessages(int batchSize, short FrameCount, CallbackDict? Callbacks = null) 
        { 
            throw new NotImplementedException();
        }
    }

    internal class SubscriberClient: BaseClient
    {
        private readonly SubscriberSocket socket;
        public SubscriberClient(string SocketURI): base()
        {
            socket = new(SocketURI);
        }
        public void Subscribe(string Topic)
        {
            socket.Subscribe(Topic);
        }

        public override MQMessageBuffer CollectAndInvokeMQMessages(int batchSize, short FrameCount, CallbackDict? Callbacks = null)
        {
            return CollectAndInvokeMQMessagesByBatch(socket, batchSize, FrameCount, Callbacks ?? new CallbackDict());
        }
    }

    internal class PublisherClient: BaseClient
    {
        private readonly PublisherSocket socket;
        public PublisherClient(string SocketURI)
        {
            socket = new(SocketURI);
        }

        public void SendMQMessage(NetMQMessage message)
        {
            SendMQMessage(socket, message);
        }
    }

    internal class PushClient : BaseClient
    {
        private readonly PushSocket socket;
        public PushClient(string SocketURI)
        {
            socket = new(SocketURI);
        }

        public void SendMQMessage(NetMQMessage message)
        {
            SendMQMessage(socket, message);
        }
    }

    internal class PullClient: BaseClient
    {
        private readonly PullSocket socket;
        public PullClient(string SocketURI)
        {
            socket = new(SocketURI);
        }

        public override MQMessageBuffer CollectAndInvokeMQMessages(int batchSize, short FrameCount, CallbackDict? Callbacks = null)
        {
            return CollectAndInvokeMQMessagesByBatch(socket, batchSize, FrameCount, Callbacks ?? new CallbackDict());
        }
    }
}
