using AV00_Shared.Configuration;
using System.Collections.Specialized;
using System.Configuration;
using Transport.Generics;
using Transport.Event;

namespace Transport.Client
{
    public class TransportClient : BaseTransportClient, ITransportClient
    {
        private readonly PushClient transportRelayClient;

        public TransportClient(string SubscriberEventSocket, string PushEventSocket, short TransportMessageFrameCount) : base(
            new SubscriberClient($">{SubscriberEventSocket}"),
            TransportMessageFrameCount
        )
        {
            transportRelayClient = new($">{PushEventSocket}");
        }
 
        public TransportClient(ConnectionStringSettingsCollection Connections, NameValueCollection Settings) : base(
            new SubscriberClient($">{Connections["EventReceiptSocket"].ConnectionString}"),
            short.Parse(Settings["TransportMessageFrameCount"] ?? throw new Exception())
        )
        {
            transportRelayClient = new($">{Connections["TransportRelayClientSocket"].ConnectionString}");
        }

        public TransportClient(IConfigurationService Configuration) : base(
            new SubscriberClient($">{Configuration.ConnectionStrings["EventReceiptSocket"].ConnectionString}"),
            short.Parse(Configuration.AppSettings["TransportMessageFrameCount"] ?? throw new Exception())
        )
        {
            transportRelayClient = new($">{Configuration.ConnectionStrings["TransportRelayClientSocket"].ConnectionString}");
        }

        public void PushEvent(IEvent Event)
        {
            transportRelayClient.SendMQMessage(Event.Serialize());
        }

        public async Task PushEventAsync(IEvent Event)
        {
            await Task.Run(() => transportRelayClient.SendMQMessage(Event.Serialize()));
        }
    }
}
