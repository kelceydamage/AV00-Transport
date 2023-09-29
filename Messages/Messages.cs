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
        private T? data;

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

        public Event<TaskExecution> GenerateReceipt(EnumTaskEventProcessingState ExecutionState)
        {
            return new Event<TaskExecution>(ServiceName, new TaskExecution(ExecutionState), EnumEventType.EventReceipt, Id);
        }
    }

    public readonly struct TaskExecution
    {
        public EnumTaskEventProcessingState State { get => state; }
        private readonly EnumTaskEventProcessingState state;

        public TaskExecution(EnumTaskEventProcessingState ExecutionState)
        {
            state = ExecutionState;
        }
    }

    public enum EnumTaskEventProcessingState
    {
        Unprocessed,
        Processing,
        Completed,
        Error,
        Cancelled
    }

    public enum EnumEventType
    {
        Event,
        EventReceipt
    }
}
