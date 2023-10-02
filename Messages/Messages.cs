using NetMQ;
using System.Text.Json;
using System.Threading.Tasks;

namespace Transport.Messages
{
    public class Event<T>
    {
        public EnumEventType Type { get => type; }
        private readonly EnumEventType type;
        public string ServiceName { get => serviceName; }
        private readonly string serviceName;
        public Guid Id { get => id; }
        private Guid id;
        public T? Data { get => data; }
        protected T? data;

        public Event(string ServiceName, T? Data, EnumEventType Type = EnumEventType.Event, Guid? TaskId = null)
        {
            serviceName = ServiceName;
            CreateGuidIfNull(TaskId);
            data = Data;
            type = Type;
        }

        public Event(string ServiceName, EnumEventType Type = EnumEventType.Event)
        {
            serviceName = ServiceName;
            CreateGuidIfNull();
            type = Type;
        }

        public Event(NetMQMessage WireMessage)
        {
            serviceName = WireMessage[0].ConvertToString();
            id = Guid.Parse(WireMessage[2].ConvertToString());
            type = (EnumEventType)Enum.Parse(typeof(EnumEventType), WireMessage[1].ConvertToString());
            data = JsonSerializer.Deserialize<T>(WireMessage[3].ConvertToString()) ?? throw new Exception($"Failed to deserialize event data into type({typeof(T)})");
        }

        public NetMQMessage Serialize()
        {
            NetMQMessage WireMessage = new();
            WireMessage.Append(ServiceName);
            WireMessage.Append(Type.ToString());
            WireMessage.Append(Id.ToString());
            WireMessage.Append(JsonSerializer.Serialize(Data));
            return WireMessage;
        }

        private void CreateGuidIfNull(Guid? TaskId = null)
        {
            if (TaskId != null)
            {
                id = (Guid)TaskId;
            }
            else
            {
                id = Guid.NewGuid();
            }
        }

        public static Event<T> Deserialize(NetMQMessage WireMessage)
        {
            return new(WireMessage);
        }

        public Event<TaskExecution> GenerateReceipt(EnumEventProcessingState ExecutionState, string ReasonForExecutionState)
        {
            return new Event<TaskExecution>(ServiceName, new TaskExecution(ExecutionState, ReasonForExecutionState), EnumEventType.EventReceipt, Id);
        }
    }

    public readonly struct TaskExecution
    {
        public EnumEventProcessingState State { get => state; }
        private readonly EnumEventProcessingState state;
        public string ReasonForState { get => reasonForState; }
        private readonly string reasonForState;

        public TaskExecution(EnumEventProcessingState ExecutionState, string ReasonForExecutionState = "")
        {
            state = ExecutionState;
            reasonForState = ReasonForExecutionState;
        }
    }

    public enum EnumEventProcessingState
    {
        Unprocessed,
        Processing,
        Completed,
        Rejected,
        Error,
        Cancelled
    }

    public enum EnumEventType
    {
        Event,
        EventReceipt
    }
}
