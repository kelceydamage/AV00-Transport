using NetMQ;
using NetMQ.Sockets;
using Transport.Messages;

namespace Transport.Relay
{
    public class ServiceBusRelay
    {
        private PublisherSocket publisherSocket;
        private PullSocket pullSocket;

        public ServiceBusRelay(string? PullSocketAddress, string? PublisherSockerAddress)
        {
            publisherSocket = new PublisherSocket(PublisherSockerAddress);
            pullSocket = new PullSocket(PullSocketAddress);
        }

        public void Run()
        {
            while (true)
            {
                string message = pullSocket.ReceiveFrameString();
                Console.WriteLine("Relay Received {0}", message);
                publisherSocket.SendFrame(message);
                Thread.Sleep(500);
            }
        }

        public void ReceiveFrameString()
        {
            NetMQMessage message = pullSocket.ReceiveMultipartMessage(4);
            if (message.FrameCount != TaskEvent.MessageLength)
            {
                throw new Exception($"Invalid message received: Expected {TaskEvent.MessageLength}-Frames, got {message.FrameCount}-Frames");
            }
            TaskEvent MyTask = new();
            MyTask.FromNetMQMessage(message);
            Console.WriteLine($"Relay Received {MyTask.Topic}-{MyTask.TaskId}-{MyTask.Message}");
            TaskEventReceipt MyTaskReceipt = new TaskEventReceipt(MyTask.Topic, MyTask.TaskId, EnumTaskEventProcessingState.Processed);
            publisherSocket.SendMultipartMessage(MyTaskReceipt.ToNetMQMessage());
        }
    }
}
