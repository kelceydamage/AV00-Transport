using NetMQ;
using Transport.Messages;
using Transport.Sockets;

namespace Transport.Relay
{
    public class ServiceBusRelay
    {
        Publisher PublisherSocket;
        Pull PullSocket;

        public ServiceBusRelay(string? PullSocketAddress, string? PublisherSockerAddress)
        {
            PublisherSocket = new Publisher(PublisherSockerAddress);
            PullSocket = new Pull(PullSocketAddress);
        }

        public void Run()
        {
            while (true)
            {
                string message = PullSocket.ReceiveFrameString();
                Console.WriteLine("Relay Received {0}", message);
                PublisherSocket.SendFrame(message);
                Thread.Sleep(500);
            }
        }

        public void ReceiveFrameString()
        {
            NetMQMessage message = PullSocket.ReceiveMultipartMessage(4);
            TaskEvent MyTask = TaskEvent.FromNetMQMessage(message);
            Console.WriteLine($"Relay Received {MyTask.Topic}-{MyTask.TaskId}-{MyTask.Message}");
            TaskEventReceipt MyTaskReceipt = new TaskEventReceipt(MyTask.Topic, MyTask.TaskId, TaskEventProcessingState.Processed);
            PublisherSocket.SendMultipartMessage(MyTaskReceipt.ToNetMQMessage());
        }
    }
}
