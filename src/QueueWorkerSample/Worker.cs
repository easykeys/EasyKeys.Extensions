using System;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Queue.Abstractions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QueueWorkerSample
{
    public class Worker : BackgroundService
    {
        private readonly IQueueBus _queueBus;
        private readonly ILogger<Worker> _logger;

        public Worker(IQueueBus queueBus, ILogger<Worker> logger)
        {
            _queueBus = queueBus;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _queueBus.Subscribe<SendGridEvent, SendGridQueueEventHandler>(Program.QueueName);
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _queueBus.Unsubscribe<SendGridEvent, SendGridQueueEventHandler>(Program.QueueName);
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = new SendGridEvent { Data = $"New Message dated: {DateTimeOffset.Now}" };
                await _queueBus.PublishAsync(Program.QueueName, message, stoppingToken);

                _logger.LogInformation("Message Sent at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
