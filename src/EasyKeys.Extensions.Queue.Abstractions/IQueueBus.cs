using System.Threading;
using System.Threading.Tasks;

namespace EasyKeys.Extensions.Queue.Abstractions
{
    public interface IQueueBus
    {
        Task PublishAsync(string queueName, QueueEvent queueEvent, CancellationToken cancellationToken = default);

        void Subscribe<TEvent, TEvenHandler>(string queueName)
            where TEvent : QueueEvent
            where TEvenHandler : IQueueEventHandler<TEvent>;

        void Unsubscribe<TEvent, TEvenHandler>(string queueName)
            where TEvent : QueueEvent
            where TEvenHandler : IQueueEventHandler<TEvent>;

        void SubscribeDynamic<THandler>(string queueName, string eventName)
            where THandler : IDynamicQueueEventHandler;

        void UnsubscribeDynamic<THandler>(string queueName, string eventName)
            where THandler : IDynamicQueueEventHandler;
    }
}
