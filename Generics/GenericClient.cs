using NetMQ;
using NetMQ.Sockets;

namespace Transport.Generics
{
    using MQMessageBuffer = Queue<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    public class BaseSocketClient
    {
        public BaseSocketClient() { }

        protected static MQMessageBuffer CollectAndInvokeMQMessagesByBatch<T>(T Socket, int batchSize, short FrameCount, CallbackDict Callbacks) where T: NetMQSocket
        {
            MQMessageBuffer messageBuffer = new();
            var message = new NetMQMessage();
            for (int count = 0; count < batchSize; count++)
            {
                if (!Socket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(500), message: ref message, FrameCount))
                {
                    break;
                }
                Callbacks.TryGetValue("DEFAULT", out Callback? callbackToFire); ;
                Callbacks.TryGetValue(message[0].ConvertToString(), out Callback? specificCallback);
                if (specificCallback is not null)
                    callbackToFire = specificCallback;
                    
                // TODO: Make async
                messageBuffer.Enqueue((message, callbackToFire?.Invoke(message)));
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

    public class SubscriberClient: BaseSocketClient, ISubscriber
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

    internal class BoundSubscriberClient : BaseSocketClient, ISubscriber
    {
        private readonly XSubscriberSocket socket;
        public BoundSubscriberClient(string SocketURI) : base()
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

    internal class PublisherClient: BaseSocketClient, IPublisher
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

    internal class BoundPublisherClient : BaseSocketClient, IPublisher
    {
        private readonly XPublisherSocket socket;
        public BoundPublisherClient(string SocketURI)
        {
            socket = new(SocketURI);
        }

        public void SendMQMessage(NetMQMessage message)
        {
            SendMQMessage(socket, message);
        }
    }

    public class PushClient : BaseSocketClient
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

    internal class PullClient: BaseSocketClient
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
