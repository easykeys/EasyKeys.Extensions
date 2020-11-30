using System.Threading.Tasks;

using EasyKeys.Extensions.Queue.Abstractions;

using Microsoft.Extensions.Logging;

namespace QueueWorkerSample
{
    public class SendGridQueueEventHandler : IQueueEventHandler<SendGridEvent>
    {
        private readonly ILogger<SendGridQueueEventHandler> _logger;

        public SendGridQueueEventHandler(ILogger<SendGridQueueEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(SendGridEvent queueEvent)
        {
            _logger.LogInformation(queueEvent.Data as string);

            return Task.CompletedTask;
        }
    }
}
