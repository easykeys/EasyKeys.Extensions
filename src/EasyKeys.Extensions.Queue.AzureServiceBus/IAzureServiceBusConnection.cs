
using Microsoft.Azure.ServiceBus;

namespace EasyKeys.Extensions.Queue.AzureServiceBus
{
    public interface IAzureServiceBusConnection
    {
        QueueClient CreateClient(string name);
    }
}
