using NetMQ;
using System.Text.Json;
using AV00_Shared.Models;
using AV00_Shared.FlowControl;

namespace Transport.Event
{
    public enum EnumEventType
    {
        Event,
        EventReceipt,
        EventLog,
    }

    public class Event<T>
    {
        public EnumEventType Type { get => type; }
        private readonly EnumEventType type;
        public string ServiceName { get => serviceName; }
        private readonly string serviceName;
        public Guid Id { get => id; }
        private Guid id;
        public T Model { get => model; }
        protected T model;

        public Event(IEventModel Model, EnumEventType Type = EnumEventType.Event)
        {
            serviceName = Model.ServiceName;
            id = Model.Id;
            model = (T)Model;
            type = Type;
        }

        public Event(NetMQMessage WireMessage)
        {
            serviceName = WireMessage[0].ConvertToString();
            id = Guid.Parse(WireMessage[2].ConvertToString());
            type = (EnumEventType)Enum.Parse(typeof(EnumEventType), WireMessage[1].ConvertToString());
            model = JsonSerializer.Deserialize<T>(WireMessage[3].ConvertToString()) ?? throw new Exception($"Failed to deserialize event data into type({typeof(T)})");
        }

        public NetMQMessage Serialize()
        {
            NetMQMessage WireMessage = new();
            WireMessage.Append(ServiceName);
            WireMessage.Append(Type.ToString());
            WireMessage.Append(Id.ToString());
            WireMessage.Append(JsonSerializer.Serialize(Model));
            return WireMessage;
        }

        public static Event<T> Deserialize(NetMQMessage WireMessage)
        {
            return new(WireMessage);
        }

        public Event<TaskExecutionEventModel> GenerateReceipt(EnumEventProcessingState ExecutionState, string ReasonForExecutionState)
        {
            return new Event<TaskExecutionEventModel>(new TaskExecutionEventModel(ServiceName, ExecutionState, ReasonForExecutionState), EnumEventType.EventReceipt);
        }
    }
}
