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
        public EnumEventType EventType { get; }
        public NetMQMessage ToNetMQMessage();
        public void FromNetMQMessage(NetMQMessage netMessage);
    }

    public struct TaskEvent: IEvent
    {
        public readonly EnumEventType EventType { get => EnumEventType.TaskEvent; }
        public readonly string Topic { get => topic; }
        private string topic;
        public readonly Guid TaskId { get => taskId; }
        private  Guid taskId;
        public readonly DataDict? Data { get => data; }
        private DataDict? data;

        public TaskEvent(string Topic, DataDict DataDict, Guid? TaskId=null)
        {
            topic = Topic;
            if (TaskId != null)
            {
                taskId = (Guid)TaskId;
            }
            else
            {
                taskId = Guid.NewGuid();
            }
            data = DataDict;
        }

        public readonly NetMQMessage ToNetMQMessage()
        {
            NetMQMessage MQMessage = new();
            MQMessage.Append(Topic);
            MQMessage.Append(EventType.ToString());
            MQMessage.Append(TaskId.ToString());
            MQMessage.Append(JsonSerializer.Serialize(Data));
            return MQMessage;
        }

        public void FromNetMQMessage(NetMQMessage MQMessage)
        {
            topic = MQMessage[0].ConvertToString();
            taskId = Guid.Parse(MQMessage[2].ConvertToString());
            data = JsonSerializer.Deserialize<DataDict>(MQMessage[3].ConvertToString());
        }
    }

    public struct TaskEventReceipt: IEvent
    {
        public readonly EnumEventType EventType { get => EnumEventType.TaskEventReceipt; }
        public readonly string Topic { get => topic; }
        private string topic;
        public readonly Guid TaskId { get => taskId; }
        private Guid taskId;
        public readonly EnumTaskEventProcessingState ProcessingState { get => processingState; }
        public EnumTaskEventProcessingState processingState;

        public TaskEventReceipt(string Topic, Guid TaskId, EnumTaskEventProcessingState ProcessingState)
        {
            this.topic = Topic;
            this.taskId = TaskId;
            this.processingState = ProcessingState;
        }

        public readonly NetMQMessage ToNetMQMessage()
        {
            NetMQMessage MQMessage = new();
            MQMessage.Append(Topic);
            MQMessage.Append(EventType.ToString());
            MQMessage.Append(TaskId.ToString());
            MQMessage.Append(ProcessingState.ToString());
            return MQMessage;
        }

        public void FromNetMQMessage(NetMQMessage MQMessage)
        {
            topic = MQMessage[0].ConvertToString();
            taskId = Guid.Parse(MQMessage[2].ConvertToString());
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
