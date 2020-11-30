
using EasyKeys.Extensions.Queue.Abstractions;

namespace QueueWorkerSample
{
    public class SendGridEvent : QueueEvent
    {
        public object Data { get; set; }
    }
}
