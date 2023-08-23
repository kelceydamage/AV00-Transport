using NetMQ;

namespace Transport.Messages
{
    public interface IEvent
    {
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
        public readonly string Message { get => message; }
        private  string message;
        public const short MessageLength = 4;

        public TaskEvent(string Topic, string Message, Guid? TaskId=null)
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
            message = Message;
        }

        public NetMQMessage ToNetMQMessage()
        {
            NetMQMessage MQMessage = new NetMQMessage();
            MQMessage.Append(Topic);
            MQMessage.Append(MessageLength);
            MQMessage.Append(TaskId.ToString());
            MQMessage.Append(Message);
            return MQMessage;
        }

        public void FromNetMQMessage(NetMQMessage MQMessage)
        {
            topic = MQMessage[0].ConvertToString();
            taskId = Guid.Parse(MQMessage[2].ConvertToString());
            message = MQMessage[3].ConvertToString();
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
        public const short MessageLength = 4;

        public TaskEventReceipt(string Topic, Guid TaskId, EnumTaskEventProcessingState ProcessingState)
        {
            this.topic = Topic;
            this.taskId = TaskId;
            this.processingState = ProcessingState;
        }

        public NetMQMessage ToNetMQMessage()
        {
            NetMQMessage MQMessage = new NetMQMessage();
            MQMessage.Append(Topic);
            MQMessage.Append(MessageLength);
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
