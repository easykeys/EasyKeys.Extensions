using System;
using System.Collections.Concurrent;

using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Queue.AzureServiceBus
{
    public class AzureServiceBusConnection : IAzureServiceBusConnection
    {
        private readonly ILogger<AzureServiceBusConnection> _logger;

        private readonly IOptionsMonitor<AzureServiceBusOptions> _optionsMonitor;

        private ConcurrentDictionary<string, QueueClient> _store = new ConcurrentDictionary<string, QueueClient>();

        public AzureServiceBusConnection(
            IOptionsMonitor<AzureServiceBusOptions> optionsMonitor,
            ILogger<AzureServiceBusConnection> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _optionsMonitor = optionsMonitor;
        }

        public QueueClient CreateClient(string name)
        {
            var options = _optionsMonitor.Get(name);
            if (_store.TryGetValue(name, out var client))
            {
                _logger.LogInformation("[AzureServiceBus] Reused from cache QueueClient");
                return client;
            }

            _logger.LogInformation("[AzureServiceBus] Creating QueueClient");
            var queueClient = new QueueClient(options.ConnectionBuilder, ReceiveMode.PeekLock, RetryPolicy.Default);
            _store.AddOrUpdate(name, queueClient, (_, a) => a);
            return queueClient;
        }
    }
}
