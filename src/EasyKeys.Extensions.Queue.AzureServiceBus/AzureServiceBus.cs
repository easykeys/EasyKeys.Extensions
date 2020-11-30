using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Queue.Abstractions;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyKeys.Extensions.Queue.AzureServiceBus
{
    public class AzureServiceBus : IQueueBus, IDisposable
    {
        private const string _QUEUEEVENTSUFFIX = "QueueEvent";

        private readonly IServiceProvider _serviceProvider;
        private readonly IAzureServiceBusConnection _connection;
        private readonly IQueueBusSubscriptionsManager _manager;
        private readonly ILogger<AzureServiceBus> _logger;
        private readonly ConcurrentDictionary<string, QueueClient> _store = new ConcurrentDictionary<string, QueueClient>();

        public AzureServiceBus(
            IServiceProvider serviceProvider,
            IAzureServiceBusConnection connection,
            IQueueBusSubscriptionsManager manager,
            ILogger<AzureServiceBus> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            _manager?.Clear();
        }

        public async Task PublishAsync(
            string queueName,
            QueueEvent @event,
            CancellationToken cancellationToken = default)
        {
            var eventName = @event.GetType().Name.Replace(_QUEUEEVENTSUFFIX, string.Empty);
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = eventName,
            };

            var client = _connection.CreateClient(queueName);

            cancellationToken.ThrowIfCancellationRequested();

            await client.SendAsync(message);
        }

        public void SubscribeDynamic<THandler>(string queueName, string eventName)
           where THandler : IDynamicQueueEventHandler
        {
            RegisterQueueEvent(queueName, eventName);

            _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName, typeof(THandler).Name);

            _manager.AddDynamicSubscription<THandler>(eventName);
        }

        public void Subscribe<TEvent, THandler>(string queueName)
            where TEvent : QueueEvent
            where THandler : IQueueEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name.Replace(_QUEUEEVENTSUFFIX, string.Empty);

            RegisterQueueEvent(queueName, eventName);

            var containsKey = _manager.HasSubscriptionsForEvent<TEvent>();
            if (!containsKey)
            {
                _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(THandler).Name);

                _manager.AddSubscription<TEvent, THandler>();
            }

            _logger.LogInformation("Subscription to event {EventName} with {EventHandler} exists", eventName, typeof(THandler).Name);
        }

        public void UnsubscribeDynamic<THander>(string queueName, string eventName)
           where THander : IDynamicQueueEventHandler
        {
            UnRegisterQueueEvent(queueName, eventName);

            _logger.LogInformation("Unsubscribing from dynamic event {EventName}", eventName);

            _manager.RemoveDynamicSubscription<THander>(eventName);
        }

        public void Unsubscribe<TEvent, THandler>(string queueName)
           where TEvent : QueueEvent
           where THandler : IQueueEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name.Replace(_QUEUEEVENTSUFFIX, string.Empty);

            UnRegisterQueueEvent(queueName, eventName);

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _manager.RemoveSubscription<TEvent, THandler>();
        }

        private void UnRegisterQueueEvent(string queueName, string eventName)
        {
            var name = $"{queueName}-{eventName}";

            if (_store.TryGetValue(name, out var queueClient))
            {
                queueClient.CloseAsync().GetAwaiter().GetResult();
            }
        }

        private void RegisterQueueEvent(string queueName, string eventName)
        {
            var name = $"{queueName}-{eventName}";

            if (_store.TryGetValue(name, out var queueClient))
            {
                RegisterSubscriptionClientMessageHandler(queueClient);
            }

            var newClient = _store.AddOrUpdate(name, _connection.CreateClient(queueName), (_, __) => __);
            RegisterSubscriptionClientMessageHandler(newClient);
        }

        private void RegisterSubscriptionClientMessageHandler(QueueClient queueClient)
        {
            queueClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = message.Label; // $"{message.Label}{QUEUE_EVENT_SUFFIX}";
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    // Complete the message so that it is not received again.
                    if (await ProcessEventAsync(eventName, messageData, token))
                    {
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = 10, AutoComplete = false });
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }

        private async Task<bool> ProcessEventAsync(string eventName, string message, CancellationToken cancellationToken)
        {
            var processed = false;
            if (_manager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var subscriptions = _manager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            if (scope.ServiceProvider.GetService(subscription.HandlerType) is not IDynamicQueueEventHandler handler)
                            {
                                continue;
                            }

                            dynamic eventData = JObject.Parse(message);
                            await handler?.HandleAsync(eventData, cancellationToken);
                        }
                        else
                        {
                            var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                            if (handler == null)
                            {
                                continue;
                            }

                            var eventType = _manager.GetEventTypeByName(eventName);
                            var queueEvent = JsonConvert.DeserializeObject(message, eventType);
                            var concreteType = typeof(IQueueEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { queueEvent });
                        }
                    }
                }

                processed = true;
            }

            return processed;
        }
    }
}
