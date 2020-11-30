using System.Threading.Tasks;

namespace EasyKeys.Extensions.Queue.Abstractions
{
    public interface IQueueEventHandler<in TQueueEvent> : IQueueEventHandler
       where TQueueEvent : QueueEvent
    {
        Task Handle(TQueueEvent queueEvent);
    }

    public interface IQueueEventHandler
    {
    }
}
