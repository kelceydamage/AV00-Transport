using NetMQ;

namespace Transport.Messages
{
    public readonly struct TaskEvent
    {
        public readonly string Topic;
        public readonly Guid TaskId;
        public readonly string Message;
        public const short MessageLength = 4;

        public TaskEvent(string Topic, string Message, Guid? TaskId=null)
        {
            this.Topic = Topic;
            if (TaskId != null)
            {
                this.TaskId = (Guid)TaskId;
            }
            else
            {
                this.TaskId = Guid.NewGuid();
            }
            this.TaskId = Guid.NewGuid();
            this.Message = Message;
        }

        public NetMQMessage ToNetMQMessage()
        {
            NetMQMessage message = new NetMQMessage();
            message.Append(Topic);
            message.Append(MessageLength);
            message.Append(TaskId.ToString());
            message.Append(Message);
            return message;
        }

        public static TaskEvent FromNetMQMessage(NetMQMessage message)
        {
            return new TaskEvent(
                Topic: message[0].ConvertToString(),
                TaskId: Guid.Parse(message[2].ConvertToString()),
                Message: message[3].ConvertToString()
            );
        }
    }

    public struct TaskEventReceipt
    {
        public readonly string Topic;
        public readonly Guid TaskId;
        public TaskEventProcessingState ProcessingState;
        public const short MessageLength = 4;

        public TaskEventReceipt(string Topic, Guid TaskId, TaskEventProcessingState ProcessingState)
        {
            this.Topic = Topic;
            this.TaskId = TaskId;
            this.ProcessingState = ProcessingState;
        }

        public NetMQMessage ToNetMQMessage()
        {
            NetMQMessage message = new NetMQMessage();
            message.Append(Topic);
            message.Append(MessageLength);
            message.Append(TaskId.ToString());
            message.Append(ProcessingState.ToString());
            return message;
        }

        public static TaskEventReceipt FromNetMQMessage(NetMQMessage message)
        {
            return new TaskEventReceipt(
                Topic: message[0].ConvertToString(),
                TaskId: Guid.Parse(message[2].ConvertToString()),
                ProcessingState: (TaskEventProcessingState)Enum.Parse(typeof(TaskEventProcessingState), message[3].ConvertToString())
            );
        }
    }

    public enum TaskEventProcessingState
    {
        Unprocessed,
        Processing,
        Processed,
        Error
    }
}
