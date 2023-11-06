using AV00_Shared.Logging;
using AV00_Shared.Models;
using Transport.Generics;
using Transport.Event;

namespace Transport.Client
{
    using LogEvent = Event<LogEventModel>;
    public class EventLogger
    {
        private readonly IPublisher logPublisher;
        public EventLogger(IPublisher Publisher)
        {
            logPublisher = Publisher;
        }

        public void Log(string ServiceName, EnumLogMessageType LogType, string Message)
        {
            LogEventModel logMessage = new(ServiceName, LogType, Message);
            LogEvent logEvent = new(logMessage);
            logPublisher.SendMQMessage(logEvent.Serialize());
            Console.WriteLine(logMessage.ToStringMessage());
        }
    }
}
