using NetMQ;
using System.Text.Json;

namespace Transport.Messages
{
    using DataDict = Dictionary<string, object>;

    public static class Event
    {
        static readonly Dictionary<EnumEventType, short> FrameCountByEventType = new()
        {
            { EnumEventType.TaskEvent, 4},
            { EnumEventType.TaskEventReceipt, 4 },
        };

        public static short GetFrameCountByEventType(EnumEventType EventType)
        {
            return FrameCountByEventType[EventType];
        }
    }

    public interface IEvent
    {
        public const short FrameCount = 4;
        public EnumEventType Type { get; }
        public NetMQMessage ToNetMQMessage();
        public void FromNetMQMessage(NetMQMessage netMessage);
    }

    public struct TaskEvent: IEvent
    {
        public readonly EnumEventType Type { get => EnumEventType.TaskEvent; }
        public readonly string ServiceName { get => serviceName; }
        private string serviceName;
        public readonly Guid Id { get => id; }
        private  Guid id;
        public readonly DataDict? Data { get => data; }
        private DataDict? data;

        public TaskEvent(string ServiceName, DataDict DataDict, Guid? TaskId=null)
        {
            serviceName = ServiceName;
            if (TaskId != null)
            {
                id = (Guid)TaskId;
            }
            else
            {
                id = Guid.NewGuid();
            }
            data = DataDict;
        }

        public readonly NetMQMessage ToNetMQMessage()
        {
            NetMQMessage MQMessage = new();
            MQMessage.Append(ServiceName);
            MQMessage.Append(Type.ToString());
            MQMessage.Append(Id.ToString());
            MQMessage.Append(JsonSerializer.Serialize(Data));
            return MQMessage;
        }

        public void FromNetMQMessage(NetMQMessage MQMessage)
        {
            serviceName = MQMessage[0].ConvertToString();
            id = Guid.Parse(MQMessage[2].ConvertToString());
            data = JsonSerializer.Deserialize<DataDict>(MQMessage[3].ConvertToString());
        }

        public readonly TaskEventReceipt GenerateReceipt(EnumTaskEventProcessingState ProcessingState)
        {
            return new TaskEventReceipt(ServiceName, id, ProcessingState);
        }
    }

    public struct TaskEventReceipt: IEvent
    {
        public readonly EnumEventType Type { get => EnumEventType.TaskEventReceipt; }
        public readonly string ServiceName { get => serviceName; }
        private string serviceName;
        public readonly Guid Id { get => id; }
        private Guid id;
        public readonly EnumTaskEventProcessingState ProcessingState { get => processingState; }
        public EnumTaskEventProcessingState processingState;

        public TaskEventReceipt(string ServiceName, Guid TaskId, EnumTaskEventProcessingState ProcessingState)
        {
            serviceName = ServiceName;
            id = TaskId;
            processingState = ProcessingState;
        }

        public readonly NetMQMessage ToNetMQMessage()
        {
            NetMQMessage MQMessage = new();
            MQMessage.Append(ServiceName);
            MQMessage.Append(Type.ToString());
            MQMessage.Append(Id.ToString());
            MQMessage.Append(ProcessingState.ToString());
            return MQMessage;
        }

        public void FromNetMQMessage(NetMQMessage MQMessage)
        {
            serviceName = MQMessage[0].ConvertToString();
            id = Guid.Parse(MQMessage[2].ConvertToString());
            processingState = (EnumTaskEventProcessingState)Enum.Parse(typeof(EnumTaskEventProcessingState), MQMessage[3].ConvertToString());
        }
    }

    public enum EnumTaskEventProcessingState
    {
        Unprocessed,
        Processing,
        Processed,
        Error
    }

    public enum EnumEventType
    {
        TaskEvent,
        TaskEventReceipt
    }
}
