using NetMQ;
using NetMQ.Sockets;
using System.Net.Sockets;

namespace Transport.Generics
{
    using MQMessageBuffer = List<(NetMQMessage, bool?)>;
    using Callback = Func<NetMQMessage, bool>;
    using CallbackDict = Dictionary<string, Func<NetMQMessage, bool>>;

    internal class BaseClient
    {
        public BaseClient() { }

        protected MQMessageBuffer CollectAndInvokeMQMessagesByBatch<T>(T Socket, int batchSize, CallbackDict Callbacks) where T: NetMQSocket
        {
            MQMessageBuffer messageBuffer = new();
            var message = new NetMQMessage();
            for (int count = 0; count < batchSize; count++)
            {
                if (!Socket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(500), message: ref message, 4))
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

        protected void SendMQMessage<T>(T Socket, NetMQMessage message) where T: NetMQSocket
        {
            Socket.SendMultipartMessage(message);
        }
    }

    internal class SubscriberClient: BaseClient
    {
        private SubscriberSocket socket;
        public SubscriberClient(string SocketURI): base()
        {
            socket = new(SocketURI);
        }
        public void Subscribe(string Topic)
        {
            socket.Subscribe(Topic);
        }

        public MQMessageBuffer CollectAndInvokeMQMessages(int batchSize, CallbackDict Callbacks)
        {
            return CollectAndInvokeMQMessagesByBatch<SubscriberSocket>(socket, batchSize, Callbacks);
        }
    }

    internal class PushClient : BaseClient
    {
        private PushSocket socket;
        public PushClient(string SocketURI)
        {
            socket = new(SocketURI);
        }

        public void PushMQMessage(NetMQMessage message)
        {
            SendMQMessage<PushSocket>(socket, message);
        }
    }
}
