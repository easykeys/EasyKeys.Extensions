using System;
using System.Collections.Generic;

namespace EasyKeys.Extensions.Queue.Abstractions
{
    public interface IQueueBusSubscriptionsManager
    {
        event EventHandler<string> OnEventRemoved;

        bool IsEmpty { get; }

        void AddDynamicSubscription<TH>(string eventName) where TH : IDynamicQueueEventHandler;

        void AddSubscription<T, TH>() where T : QueueEvent where TH : IQueueEventHandler<T>;

        void RemoveSubscription<T, TH>() where TH : IQueueEventHandler<T> where T : QueueEvent;

        void RemoveDynamicSubscription<TH>(string eventName) where TH : IDynamicQueueEventHandler;

        bool HasSubscriptionsForEvent<T>() where T : QueueEvent;

        bool HasSubscriptionsForEvent(string eventName);

        Type GetEventTypeByName(string eventName);

        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : QueueEvent;

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        string GetEventKey<T>();
    }
}
