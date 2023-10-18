using AV00_Shared.Configuration;
using System.Collections.Specialized;
using System.Configuration;
using Transport.Generics;
using Transport.Event;

namespace Transport.Client
{
    public class TransportClient : BaseTransportClient, ITransportClient
    {
        private readonly PushClient ServiceBusProducer;

        public TransportClient(string SubscriberEventSocket, string PushEventSocket, short TransportMessageFrameCount) : base(
            new SubscriberClient($">{SubscriberEventSocket}"),
            TransportMessageFrameCount
        )
        {
            ServiceBusProducer = new($">{PushEventSocket}");
        }
 
        public TransportClient(ConnectionStringSettingsCollection Connections, NameValueCollection Settings) : base(
            new SubscriberClient($">{Connections["SubscribeEventSocket"].ConnectionString}"),
            short.Parse(Settings["TransportMessageFrameCount"] ?? throw new Exception())
        )
        {
            ServiceBusProducer = new($">{Connections["PushEventSocket"].ConnectionString}");
        }

        public TransportClient(IConfigurationService Configuration) : base(
            new SubscriberClient($">{Configuration.ConnectionStrings["SubscribeEventSocket"].ConnectionString}"),
            short.Parse(Configuration.AppSettings["TransportMessageFrameCount"] ?? throw new Exception())
        )
        {
            ServiceBusProducer = new($">{Configuration.ConnectionStrings["PushEventSocket"].ConnectionString}");
        }

        public void PushEvent(IEvent Event)
        {
            ServiceBusProducer.SendMQMessage(Event.Serialize());
        }

        public async Task PushEventAsync(IEvent Event)
        {
            await Task.Run(() => ServiceBusProducer.SendMQMessage(Event.Serialize()));
        }
    }
}
