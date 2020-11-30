using Microsoft.Azure.ServiceBus;

namespace EasyKeys.Extensions.Queue.AzureServiceBus
{
    public class AzureServiceBusOptions
    {
        public string QueueName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        internal ServiceBusConnectionStringBuilder? ConnectionBuilder { get; set; } = null;

        public void Build()
        {
            if (ConnectionBuilder == null)
            {
                // TODO: add ability to pass endpoint, sharedaccesskeyname,  sharedaccesskey separate.
                ConnectionBuilder = new ServiceBusConnectionStringBuilder(ConnectionString)
                {
                    EntityPath = QueueName
                };
            }
        }
    }
}
